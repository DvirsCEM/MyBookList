using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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
        if (request.Name == "getItems")
        {
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

  // static void 
}


class Database() : DatabaseCore("database")
{
  public DbSet<User> Users { get; set; } = default!;
  public DbSet<Book> Books { get; set; } = default!;
  public DbSet<Favorite> Favorites { get; set; } = default!;
}

class User(string id, string username, string password)
{
  [Key] public string Id { get; set; } = id;
  public string Username { get; set; } = username;
  public string Password { get; set; } = password;
}

class Book(
  string title,
  string author,
  string imageSource,
  string description,
  string uploaderId
)
{
  [Key] public int Id { get; set; } = default!;
  public string Title { get; set; } = title;
  public string Author { get; set; } = author;
  public string ImageSource { get; set; } = imageSource;
  public string Description { get; set; } = description;
  public string UploaderId { get; set; } = uploaderId;
  [ForeignKey("UploaderId")] public User Uploader { get; set; } = default!;
}

class Favorite(string userId, int bookId)
{
  [Key] public int Id { get; set; } = default!;

  public string UserId { get; set; } = userId;
  [ForeignKey("UserId")] public User User { get; set; } = default!;

  public int BookId { get; set; } = bookId;
  [ForeignKey("BookId")] public Book Book { get; set; } = default!;
}