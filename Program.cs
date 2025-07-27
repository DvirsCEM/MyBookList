using System;
using System.ComponentModel.DataAnnotations;
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
          var Items = database.Items.ToList();
          request.Respond(Items);
        }
        else if (request.Name == "addItem")
        {
          var (name, price) = request.GetParams<(string, double)>();
          var Item = new Item(name, price);
          database.Items.Add(Item);
          database.SaveChanges();
          request.Respond(Item.Id);
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


class Database() : DatabaseCore("database")
{
  /*──────────────────────────────╮
  │ Add your database tables here │
  ╰──────────────────────────────*/
  public DbSet<Item> Items { get; set; } = default!;
}

class Item(string name, double price)
{
  [Key] public int Id { get; set; } = default!;
  public string Name { get; set; } = name;
  public double Price { get; set; } = price;
}