using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace App.Util.Extensions
{
    public static class EnumerableExtension
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || source.Count() == 0)
                return false;
            if (predicate == null)
                return false;

            if (source.Count(predicate) > 0)
                return true;
            else
                return false;
        }
    }
}
