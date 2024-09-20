using System;
using System.Collections.Generic;

namespace Appegy.Storage
{
    internal static class CollectionExtensions
    {
        #region AddRange

        public static void AddRange<T>(this ICollection<T> source, T item1, T item2)
        {
            source.Add(item1);
            source.Add(item2);
        }

        public static void AddRange<T>(this ICollection<T> source, T item1, T item2, T item3)
        {
            source.Add(item1);
            source.Add(item2);
            source.Add(item3);
        }

        public static void AddRange<T>(this ICollection<T> source, T item1, T item2, T item3, T item4)
        {
            source.Add(item1);
            source.Add(item2);
            source.Add(item3);
            source.Add(item4);
        }

        public static void AddRange<T>(this ICollection<T> source, params T[] items)
        {
            items.ForEach(source.Add);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, (TKey Key, TValue Value) item1, (TKey Key, TValue Value) item2)
        {
            source.Add(item1.Key, item1.Value);
            source.Add(item2.Key, item2.Value);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, (TKey Key, TValue Value) item1, (TKey Key, TValue Value) item2, (TKey Key, TValue Value) item3)
        {
            source.Add(item1.Key, item1.Value);
            source.Add(item2.Key, item2.Value);
            source.Add(item3.Key, item3.Value);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, (TKey Key, TValue Value) item1, (TKey Key, TValue Value) item2, (TKey Key, TValue Value) item3,
            (TKey Key, TValue Value) item4)
        {
            source.Add(item1.Key, item1.Value);
            source.Add(item2.Key, item2.Value);
            source.Add(item3.Key, item3.Value);
            source.Add(item4.Key, item4.Value);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, params (TKey Key, TValue Value)[] items)
        {
            items.ForEach(item => source.Add(item.Key, item.Value));
        }

        #endregion

        public static bool IsCollection(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            return typeof(ICollection<>).IsAssignableFrom(genericTypeDefinition);
        }

        public static void ForEach<T>(this Span<T> source, Action<T> predicate)
        {
            foreach (var item in source)
            {
                predicate(item);
            }
        }

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