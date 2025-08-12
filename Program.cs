using System;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Project.DatabaseUtilities;
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

    AddDefaultBooks(database);

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

          var author = database.Authors.Seek(author => author.Name == authorName)
            ?? database.Authors.Add(new Author(authorName)).Entity;

          var book = new Book(title, image, description, author.Id, userId);
          database.Books.Add(book);
          database.SaveChanges();
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
            .Seek(book => book.Id == bookId)!;
          request.Respond(book);
        }
        else if (request.Name == "getFavorites")
        {
          var userId = request.GetParams<string>();
          var favorites = database.Favorites
            .Where(favorite => favorite.UserId == userId)
            .Select(f => f.Book)
            .ToList();

          request.Respond(favorites);
        }
        else if (request.Name == "addFavorite")
        {
          var (userId, bookId) = request.GetParams<(string, int)>();

          var favorite = new Favorite(userId, bookId);
          database.Favorites.Add(favorite);
          database.SaveChanges();
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

  static void AddBook(
    Database database,
    string title,
    string authorName,
    string imageUrl,
    string description,
    string userId
    )
  {
    var author = database.Authors.Seek(author => author.Name == authorName)
      ?? database.Authors.Add(new Author(authorName)).Entity;

    var image = Convert.ToBase64String(System.IO.File.ReadAllBytes(imageUrl));

    database.Books.Add(new Book(title, image, description, author.Id, userId));
  }

  static void AddDefaultBooks(Database database)
  {
    var deafultUser = database.Users.Add(new User(Guid.NewGuid().ToString(), "Defualt User", Guid.NewGuid().ToString())).Entity;

    AddBook(
      database,
      "The Great Gatsby",
      "F. Scott Fitzgerald",
      "images/TheGreatGatsby.jpg",
      "A novel set in the 1920s about the mysterious Jay Gatsby and his obsession with Daisy Buchanan.",
      deafultUser.Id
    );
    AddBook(
      database,
      "To Kill a Mockingbird",
      "Harper Lee",
      "images/ToKillAMockingbird.jpg",
      "A novel about the serious issues of racial inequality and moral growth, seen through the eyes of young Scout Finch.",
      deafultUser.Id
    );
    AddBook(
      database,
      "1984",
      "George Orwell",
      "images/1984.jpg",
      "A dystopian novel that explores the dangers of totalitarianism and extreme political ideology.",
      deafultUser.Id
    );
    AddBook(
      database,
      "Pride and Prejudice",
      "Jane Austen",
      "images/PrideAndPrejudice.jpg",
      "A romantic novel that critiques the British landed gentry at the end of the 18th century.",
      deafultUser.Id
    );
    AddBook(
      database,
      "The Catcher in the Rye",
      "J.D. Salinger",
      "images/TheCatcherInTheRye.jpg",
      "A novel about teenage angst and alienation, narrated by the iconic character Holden Caulfield.",
      deafultUser.Id
    );

    database.SaveChanges();
  }
}


class Database() : DbCore("database")
{
  public DbSet<User> Users { get; set; } = default!;
  public DbSet<Book> Books { get; set; } = default!;
  public DbSet<Author> Authors { get; set; } = default!;
  public DbSet<Favorite> Favorites { get; set; } = default!;
}

class User(string id, string username, string password)
{
  [JsonIgnore] public string Id { get; set; } = id;
  public string Username { get; set; } = username;
  [JsonIgnore] public string Password { get; set; } = password;
}

class Book
{
    public int Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Image { get; set; } = default!;
    public string Description { get; set; } = default!;
    public Author Author { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public User User { get; set; } = default!;

    public Book(string title, string image, string description, Author author, User user)
    {
        Title = title;
        Image = image;
        Description = description;
        Author = author;
        User = user;
    }
}


class Author(string name)
{
  public int Id { get; set; }

  public string Name { get; set; } = name;
}

class Favorite(string userId, int bookId)
{
  public int Id { get; set; } = default!;

  public string UserId { get; set; } = userId;
  public User User { get; set; } = default!;

  public int BookId { get; set; } = bookId;
  public Book Book { get; set; } = default!;
}