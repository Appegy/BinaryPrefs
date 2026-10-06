using System.Collections.Generic;

namespace Appegy.Storage
{
    internal static class TestCollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> source, params T[] items)
        {
            foreach (var item in items)
            {
                source.Add(item);
            }
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, params (TKey Key, TValue Value)[] items)
        {
            foreach (var item in items)
            {
                source.Add(item.Key, item.Value);
            }
        }
    }
}
