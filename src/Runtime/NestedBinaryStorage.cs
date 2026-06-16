using System;
using System.Collections.Generic;
using System.Linq;

namespace Appegy.Storage
{
    internal class NestedBinaryStorage : IBinaryStorage
    {
        private readonly IBinaryStorage _root;
        private readonly string _prefix;

        public NestedBinaryStorage(IBinaryStorage root, string prefix)
        {
            _prefix = $"__{prefix}->";
            _root = root;
        }

        public IReadOnlyCollection<string> Keys
        {
            get
            {
                return _root.Keys
                    .Where(k => k.StartsWith(_prefix, StringComparison.Ordinal))
                    .Select(k => k.Substring(_prefix.Length))
                    .ToArray();
            }
        }

        private string GetKey(string key)
        {
            return _prefix + key;
        }

        private bool TryExtractKey(string key, out string value)
        {
            if (key.StartsWith(_prefix, StringComparison.Ordinal))
            {
                value = key.Substring(_prefix.Length);
                return true;
            }
            value = null;
            return false;
        }

        public object GetRaw(string key)
        {
            return _root.GetRaw(GetKey(key));
        }

        public bool SetRaw(string key, object value, TypeMismatchBehaviour? overrideTypeMismatchBehaviour = null)
        {
            return _root.SetRaw(GetKey(key), value, overrideTypeMismatchBehaviour);
        }

        public bool Has(string key)
        {
            return _root.Has(GetKey(key));
        }

        public Type TypeOf(string key)
        {
            return _root.TypeOf(GetKey(key));
        }

        public bool Supports<T>()
        {
            return _root.Supports<T>();
        }

        public T Get<T>(string key, T defaultValue = default, MissingKeyBehavior? overrideMissingKeyBehavior = null)
        {
            return _root.Get(GetKey(key), defaultValue, overrideMissingKeyBehavior);
        }

        public bool Set<T>(string key, T value, TypeMismatchBehaviour? overrideTypeMismatchBehaviour = null)
        {
            return _root.Set(GetKey(key), value, overrideTypeMismatchBehaviour);
        }

        public bool Remove(string key)
        {
            return _root.Remove(GetKey(key));
        }

        public int Remove(Func<string, bool> predicate)
        {
            return _root.Remove(key => TryExtractKey(key, out var value) && predicate(value));
        }

        public int RemoveAll()
        {
            return _root.Remove(key => key.StartsWith(_prefix, StringComparison.Ordinal));
        }

        public void Save()
        {
            _root.Save();
        }

        public IDisposable MultipleChangeScope()
        {
            return _root.MultipleChangeScope();
        }

        public bool SupportsListsOf<T>()
        {
            return _root.SupportsListsOf<T>();
        }

        public bool SupportsSetsOf<T>()
        {
            return _root.SupportsSetsOf<T>();
        }

        public bool SupportsDictionariesOf<TKey, TValue>()
        {
            return _root.SupportsDictionariesOf<TKey, TValue>();
        }

        public IList<T> GetListOf<T>(string key)
        {
            return _root.GetListOf<T>(GetKey(key));
        }

        public IReadOnlyList<T> GetReadOnlyListOf<T>(string key)
        {
            return _root.GetReadOnlyListOf<T>(GetKey(key));
        }

        public ISet<T> GetSetOf<T>(string key)
        {
            return _root.GetSetOf<T>(GetKey(key));
        }

        public IReadOnlyCollection<T> GetReadOnlySetOf<T>(string key)
        {
            return _root.GetReadOnlySetOf<T>(GetKey(key));
        }

        public IDictionary<TKey, TValue> GetDictionaryOf<TKey, TValue>(string key)
        {
            return _root.GetDictionaryOf<TKey, TValue>(GetKey(key));
        }

        public IReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionaryOf<TKey, TValue>(string key)
        {
            return _root.GetReadOnlyDictionaryOf<TKey, TValue>(GetKey(key));
        }
    }
}