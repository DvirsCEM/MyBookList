using System;
using System.ComponentModel.Design;
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
          var userSecret = request.GetParams<string>();
          var user = database.Users.Seek(user => user.Secret == userSecret);

          request.Respond(user != null);
        }
        else if (request.Name == "getUsername")
        {
          var userId = request.GetParams<int>();
          var user = database.Users.Find(userId);
          if (user != null)
          {
            request.Respond(user.Username);
          }
        }
        else if (request.Name == "addBook")
        {
          var (title, authorName, image, description, userSecret) = request.GetParams<(string, string, string, string, string)>();

          var author = database.Authors.Seek(author => author.Name == author.Name);

          if (author == null)
          {
            author = new Author(authorName);
            database.Authors.Add(author);
            database.SaveChanges();
          }

          var user = database.Users.Seek(user => user.Secret == userSecret)!;

          var book = new Book(title, image, description, author.Id, user.Id);
          database.Books.Add(book);
          database.SaveChanges();
        }
        else if (request.Name == "getBooks")
        {
          var books = database.Books.ToList();
          request.Respond(books);
        }
        else if (request.Name == "getFavorites")
        {
          var userId = request.GetParams<int>();
          var favorites = database.Favorites
            .Where(f => f.User.Id == userId)
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
}


class Database() : DbCore("database")
{
  public DbSet<User> Users { get; set; } = default!;
  public DbSet<Book> Books { get; set; } = default!;
  public DbSet<Author> Authors { get; set; } = default!;
  public DbSet<Favorite> Favorites { get; set; } = default!;
}

class User(string username, string password, string secret)
{
  public int Id { get; set; } = default!;
  public string Username { get; set; } = username;
  [JsonIgnore] public string Password { get; set; } = password;
  [JsonIgnore] public string Secret { get; set; } = secret; 
}

class Book(
  string title,
  string image,
  string description,
  int authorId,
  int uploaderId
)
{
  public int Id { get; set; } = default!;
  public string Title { get; set; } = title;
  public string Image { get; set; } = image;
  public string Description { get; set; } = description;

  public int AuthorId { get; set; } = authorId;
  public Author Author { get; set; } = default!;

  public int UploaderId { get; set; } = uploaderId;
  public User Uploader { get; set; } = default!;
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