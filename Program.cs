using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Project.DatabaseUtilities;
using Project.GeneralUtilities;
using Project.GenerationUtilities;
using Project.LoggingUtilities;
using Project.ServerUtilities;

class Program
{
  static void Main()
  {
    int port = 5000;

    var server = new Server(port);

    Console.WriteLine("The server is running");
    Console.WriteLine($"Main Page: http://localhost:{port}/website/pages/index.html");

    var database = new Database();

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
        /*──────────────────────────────────╮
        │ Handle your custome requests here │
        ╰──────────────────────────────────*/
        if (request.Name == "signUp")
        {
          var (username, password) = request.GetParams<(string, string)>();

          var userExists = database.Users.Any(u => u.Username == username);
          if (!userExists)
          {
            var id = Guid.NewGuid().ToString();
            var user = new User(id, username, password);
            database.Users.Add(user);
            database.SaveChanges();

            request.Respond(id);
          }
        }
        else if (request.Name == "logIn")
        {
          var (username, password) = request.GetParams<(string, string)>();
          var user = database.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
          if (user != null)
          {
            request.Respond(user.Id);
          }
        }
        else if (request.Name == "verifyUser")
        {
          var userId = request.GetParams<string>();
          var user = database.Users.Find(userId);

          request.Respond(user != null);
        }
        else if (request.Name == "getUsername")
        {
          var userId = request.GetParams<string>();
          var user = database.Users.Find(userId);
          if (user != null)
          {
            request.Respond(user.Username);
          }
        }
        else if (request.Name == "addBook")
        {
          var (title, authorName, image, description, userId) = request.GetParams<(string, string, string, string, string)>();

          AddBook(database, title, authorName, image, description, userId);
        }
        else if (request.Name == "getBooks")
        {
          var books = database.Books.ToList();
          request.Respond(books);
        }
        else if (request.Name == "getBook")
        {
          var bookId = request.GetParams<int>();
          var book = database.Books
            .Include(book => book.Author)
            .Include(book => book.Uploader)
            .Seek(book => book.Id == bookId)!;
          request.Respond(book);
        }
        else if (request.Name == "getUserBooks")
        {
          var username = request.GetParams<string>();

          var user = database.Users.Seek(user => user.Username == username)!;

          var uploadedBooks = database.Books
            .Where(book => book.Uploader.Username == username);

          var favoriteBooks = database.Favorites
            .Where(favorite => favorite.User.Username == username)
            .Select(Favorite => Favorite.Book);

          request.Respond((uploadedBooks, favoriteBooks));
        }
        else if (request.Name == "getRatingScore")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var rating = database.Ratings
            .Seek(rating => rating.User.Id == userId && rating.Book.Id == bookId);

          request.Respond(rating?.Score);
        }
        else if (request.Name == "rate")
        {
          var (score, userId, bookId) = request.GetParams<(int, string, int)>();

          var user = database.Users.Find(userId)!;
          var book = database.Books.Find(bookId)!;

          var rating = database.Ratings
            .Seek(rating => rating.User.Id == userId && rating.Book.Id == bookId);

          if (rating != null)
          {
            rating.Score = score;
          }
          else
          {
            database.Add(new Rating(score, user, book));
          }

          database.SaveChanges();
        }
        else if (request.Name == "removeRating")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var rating = database.Ratings
            .Seek(rating => rating.User.Id == userId && rating.Book.Id == bookId);

          if (rating != null)
          {
            database.Ratings.Remove(rating);
            database.SaveChanges();
          }
        }
        else if (request.Name == "getIsFavorite")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var favorite = database.Favorites
            .Seek(favorite => favorite.User.Id == userId && favorite.Book.Id == bookId);

          request.Respond(favorite != null);
        }
        else if (request.Name == "addFavorite")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var user = database.Users.Find(userId)!;
          var book = database.Books.Find(bookId)!;

          var favorite = new Favorite(user, book);
          database.Favorites.Add(favorite);
          database.SaveChanges();
        }
        else if (request.Name == "removeFavorite")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var favorite = database.Favorites.
            Seek(favorite => favorite.User.Id == userId && favorite.Book.Id == bookId)!;

          database.Favorites.Remove(favorite);
          database.SaveChanges();
        }
        else if (request.Name == "getAverageRatingScore")
        {
          var bookId = request.GetParams<int>();

          var scores = database.Ratings
            .Where(rating => rating.Book.Id == bookId)
            .Select(rating => rating.Score)
            .ToArray();


          var average = 0;

          if (scores != null)
          {
            foreach (var score in scores)
            {
              average += score;
            }

            average /= scores.Length;
          }

          request.Respond(average);
        }
        else if (request.Name == "getAuthorInfo")
        {
          var authorId = request.GetParams<int>();

          var authorName = database.Authors.Find(authorId)!.Name;

          var books = database.Books
            .Where(book => book.Author.Id == authorId);

          request.Respond((authorName, books));
        }
        else if (request.Name == "getAuthorNames")
        {
          var authorNames = database.Authors.Select(author => author.Name);

          request.Respond(authorNames);
        }
        else
        {
          request.SetStatusCode(405);
        }
      }
      catch (Exception exception)
      {
        request.SetStatusCode(422);
        Log.WriteException(exception);
      }
    }
  }

  static void AddBook(Database database, string title, string authorName, string image, string description, string userId)
  {
    var author = database.Authors.Seek(author => author.Name == authorName)
      ?? database.Authors.Add(new Author(authorName)).Entity;

    var user = database.Users.Find(userId)!;

    database.Books.Add(new Book(title, image, description, author, user));
    database.SaveChanges();
  }

  static void AddDefaultBooks(Database database)
  {
    var deafultUser = database.Users.Add(new User(Guid.NewGuid().ToString(), "Defualt User", Guid.NewGuid().ToString())).Entity;

    (string, string, string, string)[] booksProperties = [
      ("The Great Gatsby", "F. Scott Fitzgerald", "images/TheGreatGatsby.jpg", "A novel set in the 1920s about the mysterious Jay Gatsby and his obsession with Daisy Buchanan."),
      ("To Kill a Mockingbird", "Harper Lee", "images/ToKillAMockingbird.jpg", "A novel about the serious issues of racial inequality and moral growth, seen through the eyes of young Scout Finch."),
      ("1984", "George Orwell", "images/1984.jpg", "A dystopian novel that explores the dangers of totalitarianism and extreme political ideology."),
      ("Pride and Prejudice", "Jane Austen", "images/PrideAndPrejudice.jpg", "A romantic novel that critiques the British landed gentry at the end of the 18th century."),
      ("The Catcher in the Rye", "J.D. Salinger", "images/TheCatcherInTheRye.jpg", "A novel about teenage angst and alienation, narrated by the iconic character Holden Caulfield."),
      ("The Lord of the Rings", "J.R.R. Tolkien", "images/TheLordOfTheRings.jpg", "A fantasy novel that follows the quest of Frodo Baggins and his companions to destroy the One Ring and defeat Sauron."),
      ("Moby-Dick", "Herman Melville", "images/MobyDick.jpg", "A whaling voyage turns into an obsession with revenge against the elusive white whale, Moby Dick."),
      ("Brave New World", "Aldous Huxley", "images/BraveNewWorld.jpg", "A dystopian vision of a future society driven by technological and genetic control."),
      ("Crime and Punishment", "Fyodor Dostoevsky", "images/CrimeAndPunishment.jpg", "A psychological novel about guilt, redemption, and the mind of a murderer in 19th-century Russia."),
      ("Jane Eyre", "Charlotte Brontë", "images/JaneEyre.jpg", "An orphaned governess overcomes hardship and finds love in Victorian England."),
      ("War and Peace", "Leo Tolstoy", "images/WarAndPeace.jpg", "An epic novel chronicling the lives of Russian aristocrats during the Napoleonic Wars."),
      ("Frankenstein", "Mary Shelley", "images/Frankenstein.jpg", "A scientist creates a sentient creature, leading to tragic consequences."),
      ("Don Quixote", "Miguel de Cervantes", "images/DonQuixote.jpg", "A delusional nobleman sets out to revive chivalry, accompanied by his loyal squire."),
      ("Beloved", "Toni Morrison", "images/Beloved.jpg", "A haunting tale of slavery, memory, and motherhood in post-Civil War America."),
      ("The Hobbit", "J.R.R. Tolkien", "images/TheHobbit.jpg", "Bilbo Baggins embarks on an unexpected journey to reclaim a lost dwarf kingdom."),
      ("Fahrenheit 451", "Ray Bradbury", "images/Fahrenheit451.jpg", "A dystopian future where books are outlawed and 'firemen' burn them."),
      ("Wuthering Heights", "Emily Brontë", "images/WutheringHeights.jpg", "A dark tale of passion and revenge set on the Yorkshire moors."),
      ("The Alchemist", "Paulo Coelho", "images/TheAlchemist.jpg", "A young shepherd travels in search of a worldly treasure and discovers his destiny."),
      ("Little Women", "Louisa May Alcott", "images/LittleWomen.jpg", "The coming-of-age story of the four March sisters during the American Civil War.")
    ];

    foreach (var (title, author, imagePath, description) in booksProperties)
    {
      AddBook(database, title, author, imagePath.ToImgSrc(), description, deafultUser.Id);
    }
  }
}


class Database() : DbCore("database")
{
  public DbSet<User> Users { get; set; } = default!;
  public DbSet<Book> Books { get; set; } = default!;
  public DbSet<Author> Authors { get; set; } = default!;
  public DbSet<Favorite> Favorites { get; set; } = default!;
  public DbSet<Rating> Ratings { get; set; } = default!;
}

[TableSchema]
partial class User(string id, string username, string password)
{
  [JsonIgnore] public string Id { get; set; } = id;
  public string Username { get; set; } = username;
  [JsonIgnore] public string Password { get; set; } = password;
}

[TableSchema]
partial class Book(string title, string image, string description, Author author, User uploader)
{
  public int Id { get; set; } = default!;
  public string Title { get; set; } = title;
  public string Image { get; set; } = image;
  public string Description { get; set; } = description;
  public Author Author { get; set; } = author;
  public User Uploader { get; set; } = uploader;
}

[TableSchema]
partial class Author(string name)
{
  public int Id { get; set; }
  public string Name { get; set; } = name;
}

[TableSchema]
partial class Favorite(User user, Book book)
{
  public int Id { get; set; } = default!;
  public User User { get; set; } = user;
  public Book Book { get; set; } = book;
}

[TableSchema]
partial class Rating(int score, User user, Book book)
{
  public int Id { get; set; } = default!;
  public int Score { get; set; } = score;
  public User User { get; set; } = user;
  public Book Book { get; set; } = book;
}