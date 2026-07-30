using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Appegy.Storage
{
    internal static class CollectionTypeCache<T>
    {
        public static readonly bool IsCollection = CollectionTypeCache.IsCollection(typeof(T));
    }

    internal static class CollectionTypeCache
    {
        private static readonly ConcurrentDictionary<Type, bool> _cache = new();
        private static readonly Func<Type, bool> _detector = Detect;

        public static bool IsCollection(Type type)
        {
            return _cache.GetOrAdd(type, _detector);
        }

        private static bool Detect(Type type)
        {
            if (IsCollectionContract(type))
            {
                return true;
            }
            foreach (var contract in type.GetInterfaces())
            {
                if (IsCollectionContract(contract))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsCollectionContract(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>);
        }
    }
}
