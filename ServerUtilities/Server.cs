using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Project.ServerUtilities;

//
// Summary:
// Provides server utilities for handling HTTP requests and responses.
// This class allows for custom request handling and response formatting.
// It includes methods for reading request parameters and sending responses.
// It also supports custom request paths and handles static file serving.
public class Server
{
  readonly HttpListener _listener;
  HttpListenerContext? _context = null;

  //
  // Summary:
  // Initializes a new instance of the Server class with the specified port.
  //  //
  // Parameters:
  //   port:
  //     The port on which the server will listen for requests.
  //  // Returns:
  //   A new instance of the Server class.
  public Server(int port)
  {
    _listener = new HttpListener();
    _listener.Prefixes.Add($"http://*:{port}/");
    _listener.Start();
  }

  //
  // Summary:
  // Waits for an incoming request and returns a Request object representing it.
  // Returns:
  // A Request object representing the incoming request.
  public Request WaitForRequest()
  {
    while (true)
    {
      _context?.Response.Close();
      _context = _listener.GetContext()!;
      var isCustom = _context.Request.Headers["X-Custom-Request"] == "true";
      var path = GetPath(_context);
      var ext = Path.GetExtension(path).ToLowerInvariant();

      if (isCustom)
      {
        return new Request(_context, path);
      }

      if (path == "favicon.ico")
      {
        path = "website/favicon.ico";
      }

      if (!File.Exists(path))
      {
        _context.Response.StatusCode = 404;
        if (_context.Request.AcceptTypes?.Contains("text/html") ?? false)
        {
          path = "website/pages/404.html";
          _context.Response.ContentType = "text/html";
        }
        else
        {
          continue;
        }
      }
      else
      {
        _context.Response.ContentType =
           ext switch
           {
             ".html" => "text/html",
             ".js" => "application/javascript",
             ".css" => "text/css",
             ".json" => "application/json",
             ".png" => "image/png",
             ".jpg" => "image/jpeg",
             ".jpeg" => "image/jpeg",
             ".gif" => "image/gif",
             ".svg" => "image/svg+xml",
             ".ico" => "image/x-icon",
             _ => "text/html",
           };
      }

      _context.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
      _context.Response.Headers.Add("Pragma", "no-cache");
      _context.Response.Headers.Add("Expires", "Thu, 01 Jan 1970 00:00:00 GMT");

      var fileBytes = File.ReadAllBytes(path);
      _context.Response.OutputStream.Write(fileBytes);
    }
  }

  static string GetPath(HttpListenerContext context)
  {
    var path = context.Request.Url!.AbsolutePath[1..];

    var ext = Path.GetExtension(path).ToLowerInvariant();

    if (ext == "")
    {
      var referer = context.Request.UrlReferrer?.ToString();
      var refererExt = Path.GetExtension(referer)?.ToLowerInvariant();
      if (referer != null && (refererExt == ".js" || refererExt == ""))
      {
        path += ".js";
      }
    }

    return path;
  }
}
