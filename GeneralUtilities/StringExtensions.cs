using System;
using System.IO;

namespace Project.GeneralUtilities;

public static class StringExtensions
{
  public static string ToImgSrc(this string imagePath)
  {
    if (string.IsNullOrWhiteSpace(imagePath))
      throw new ArgumentException("Path cannot be null or empty.", nameof(imagePath));
    if (!File.Exists(imagePath))
      throw new FileNotFoundException("File not found.", imagePath);

    var base64 = Convert.ToBase64String(File.ReadAllBytes(imagePath));
    var ext = Path.GetExtension(imagePath).TrimStart('.').ToLowerInvariant();

    var mime = ext switch
    {
      "jpg" or "jpeg" or "jfif" => "image/jpeg",
      "png" => "image/png",
      "gif" => "image/gif",
      "bmp" => "image/bmp",
      "webp" => "image/webp",
      "svg" => "image/svg+xml",
      "tif" or "tiff" => "image/tiff",
      "ico" or "cur" => "image/x-icon",
      "avif" => "image/avif",
      "heic" => "image/heic",
      "heif" => "image/heif",
      _ => $"image/{ext}"
    };

    return $"data:{mime};base64,{base64}";
  }
}