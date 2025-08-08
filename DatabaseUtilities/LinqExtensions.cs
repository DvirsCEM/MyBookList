using System;
using System.Collections.Generic;

namespace Project.DatabaseUtilities;

public static class LinqExtensions
{
  public static T? Seek<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class
  {
    foreach (var item in source)
    {
      if (predicate(item))
        return item;
    }
    return null;
  }
}
