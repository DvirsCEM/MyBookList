using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.DatabaseUtilities;

public static class LinqExtensions
{
  public static T? Seek<T>(this IEnumerable<T> source, Func<T, bool> predicate) where T : class
  {
    return source.FirstOrDefault(predicate);
  }
}
