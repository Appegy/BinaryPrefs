using System;
using System.Collections.Generic;

namespace Appegy.BinaryStorage
{
    internal static class CollectionExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> predicate)
        {
            foreach (var item in source)
            {
                predicate(item);
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}