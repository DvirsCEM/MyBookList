using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.DatabaseUtilities;
using Project.LoggingUtilities;
using Project.ServerUtilities;

class Program
{
  static async Task Main()
  {
    int port = 5000;

    var server = new Server(port);
    var database = new Database();

    Console.WriteLine("The server is running");
    Console.WriteLine($"Local:   http://localhost:{port}/website/pages/index.html");
    Console.WriteLine($"Network: http://{Network.GetLocalNetworkIPAddress()}:{port}/website/pages/index.html");

    if (database.IsNewlyCreated)
    {
      AddDefaultBooks(database);
    }

    while (true)
    {
      var request = server.WaitForRequest();

      Console.WriteLine($"Recieved a request: {request.Name}");

      try
      {
        if (request.Name == "getUser")
        {
          var token = request.GetParams<string>();
          var user = database.Users.FirstOrDefault(u => u.Token == token);
          request.Respond(user);
        }
        else if (request.Name == "signUp")
        {
          var (username, password) = request.GetParams<(string, string)>();

          if (database.Users.Any(u => u.Username == username))
          {
            request.Respond<string?>(null);
            continue;
          }

          var token = Guid.NewGuid().ToString();
          var user = new User(token, username, password);
          database.Users.Add(user);
          database.SaveChanges();

          request.Respond(token);
        }
        else if (request.Name == "logIn")
        {
          var (username, password) = request.GetParams<(string, string)>();
          var user = database.Users.FirstOrDefault(u =>
            u.Username == username &&
            u.Password == password);

          request.Respond(user?.Token);
        }
        else if (request.Name == "getAllAuthors")
        {
          request.Respond(database.Authors);
        }
        else if (request.Name == "getAuthor")
        {
          var authorId = request.GetParams<int>();
          var author = database.Authors.First(author => author.Id == authorId);
          request.Respond(author);
        }
        else if (request.Name == "addBook")
        {
          var (title, authorName, imageUrl, description) = request.GetParams<(string, string, string, string)>();
          AddBook(database, title, authorName, imageUrl, description);
        }
        else if (request.Name == "getBook")
        {
          var bookId = request.GetParams<int>();
          var book = database.Books
            .Include(book => book.Author)
            .FirstOrDefault(book => book.Id == bookId);

          request.Respond(book);
        }
        else if (request.Name == "getAllBooks")
        {
          request.Respond(database.Books);
        }
        else if (request.Name == "getBooksByAuthor")
        {
          var authorId = request.GetParams<int>();
          var books = database.Books
            .Include(book => book.Author)
            .Where(book => book.AuthorId == authorId);
          request.Respond(books);
        }
        else if (request.Name == "setFavorite")
        {
          var (token, bookId, toAdd) = request.GetParams<(string, int, bool)>();

          var favorite = database.Favorites.FirstOrDefault(f => f.User.Token == token && f.BookId == bookId);
          if (toAdd)
          {
            if (favorite == null)
            {
              var user = database.Users.FirstOrDefault(u => u.Token == token)!;
              favorite = new Favorite(user.Id, bookId);
              database.Favorites.Add(favorite);
              database.SaveChanges();
            }
          }
          else
          {
            if (favorite != null)
            {
              database.Favorites.Remove(favorite);
              database.SaveChanges();
            }
          }
        }
        else if (request.Name == "isFavorite")
        {
          var (token, bookId) = request.GetParams<(string, int)>();
          var exists = database.Favorites.Any(f => f.User.Token == token && f.BookId == bookId);
          request.Respond(exists);
        }
        else if (request.Name == "getFavoriteBooks")
        {
          var token = request.GetParams<string>();
          var user = database.Users.FirstOrDefault(u => u.Token == token)!;
          var books = database.Favorites
            .Where(f => f.UserId == user.Id)
            .Select(f => f.Book);
          request.Respond(books);
        }
        else if (request.Name == "setRating")
        {
          var (token, bookId, score) = request.GetParams<(string, int, int)>();

          var rating = database.Ratings.FirstOrDefault(r => r.User.Token == token && r.BookId == bookId);
          if (rating == null)
          {
            var user = database.Users.FirstOrDefault(u => u.Token == token)!;
            rating = new Rating(score, user.Id, bookId);
            database.Ratings.Add(rating);
          }
          else
          {
            rating.Score = score;
          }
          database.SaveChanges();
        }
        else if (request.Name == "removeRating")
        {
          var (token, bookId) = request.GetParams<(string, int)>();
          var rating = database.Ratings.FirstOrDefault(r => r.User.Token == token && r.BookId == bookId);
          if (rating != null)
          {
            database.Ratings.Remove(rating);
            database.SaveChanges();
          }
        }
        else if (request.Name == "getPersonalScore")
        {
          var (token, bookId) = request.GetParams<(string, int)>();
          var rating = database.Ratings.FirstOrDefault(r => r.User.Token == token && r.BookId == bookId);
          if (rating == null)
          {
            request.Respond<Rating?>(null);
          }
          else
          {
            request.Respond(rating.Score);
          }
        }
        else if (request.Name == "getGlobalScore")
        {
          var bookId = request.GetParams<int>();
          var ratings = database.Ratings.Where(r => r.BookId == bookId);

          if (ratings.Count() == 0)
          {
            request.Respond<int?>(null);
            continue;
          }

          var sum = 0;
          foreach (var rating in ratings)
          {
            sum += rating.Score;
          }
          var average = sum / ratings.Count();

          request.Respond(average);
        }
        else
        {
          request.SetStatusCode(400);
        }
      }
      catch (Exception exception)
      {
        request.SetStatusCode(500);
        Log.WriteException(exception);
      }
    }
  }

  static void AddBook(Database database, string title, string authorName, string imageUrl, string description)
  {
    var author = database.Authors.FirstOrDefault(author => author.Name == authorName);
    if (author == null)
    {
      author = database.Authors.Add(new Author(authorName)).Entity;
      database.SaveChanges();
    }

    database.Books.Add(new Book(title, imageUrl, description, author.Id));
    database.SaveChanges();
  }

  static void AddDefaultBooks(Database database)
  {
    AddBook(
      database,
      "The Hobbit",
      "J.R.R. Tolkien",
      "https://m.media-amazon.com/images/S/compressed.photo.goodreads.com/books/1546071216i/5907.jpg",
      "Bilbo Baggins embarks on an unexpected journey to reclaim a lost dwarf kingdom."
    );
    AddBook(
      database,
      "The Lord of the Rings",
      "J.R.R. Tolkien",
      "https://m.media-amazon.com/images/S/compressed.photo.goodreads.com/books/1566425108i/33.jpg",
      "A fantasy novel that follows the quest of Frodo Baggins and his companions to destroy the One Ring and defeat Sauron."
    );
    AddBook(
      database,
      "1984",
      "George Orwell",
      "https://m.media-amazon.com/images/S/compressed.photo.goodreads.com/books/1657781256i/61439040.jpg",
      "A dystopian novel that explores the dangers of totalitarianism and extreme political ideology."
    );
  }
}


class Database() : DatabaseCore("database")
{
  public DbSet<User> Users { get; set; } = default!;
  public DbSet<Book> Books { get; set; } = default!;
  public DbSet<Author> Authors { get; set; } = default!;
  public DbSet<Favorite> Favorites { get; set; } = default!;
  public DbSet<Rating> Ratings { get; set; } = default!;
}

class User(string token, string username, string password)
{
  public int Id { get; set; } = default!;
  [JsonIgnore] public string Token { get; set; } = token;
  public string Username { get; set; } = username;
  [JsonIgnore] public string Password { get; set; } = password;
}

class Book(string title, string imageUrl, string description, int authorId)
{
  public int Id { get; set; } = default!;
  public string Title { get; set; } = title;
  public string ImageUrl { get; set; } = imageUrl;
  public string Description { get; set; } = description;
  public int AuthorId { get; set; } = authorId;

  public Author Author { get; set; } = default!;
}

class Author(string name)
{
  public int Id { get; set; } = default!;
  public string Name { get; set; } = name;
}

class Favorite(int userId, int bookId)
{
  public int Id { get; set; } = default!;
  public int UserId { get; set; } = userId;
  public int BookId { get; set; } = bookId;

  public User User { get; set; } = default!;
  public Book Book { get; set; } = default!;
}

class Rating(int score, int userId, int bookId)
{
  public int Id { get; set; } = default!;
  public int Score { get; set; } = score;
  public int UserId { get; set; } = userId;
  public int BookId { get; set; } = bookId;

  public User User { get; set; } = default!;
  public Book Book { get; set; } = default!;
}
