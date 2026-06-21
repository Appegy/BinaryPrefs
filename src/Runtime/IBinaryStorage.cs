using System;
using System.Collections.Generic;

namespace Appegy.Storage
{
    public interface IBinaryStorage
    {
        /// <summary> Gets all keys currently stored in the storage. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        IReadOnlyCollection<string> Keys { get; }

        /// <summary> Gets the value associated with the specified key as an untyped object. </summary>
        /// <param name="key">The key to get the value for.</param>
        /// <returns>The value associated with the key, or null if the key does not exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        object? GetRaw(string key);

        /// <summary> Sets the value for the specified key using an untyped object. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set. Its runtime type must be registered in the storage.</param>
        /// <param name="overrideTypeMismatchBehaviour">Override default behavior when the key already exists with a different type.</param>
        /// <returns>True if the value was set; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="ArgumentNullException">Thrown if value is null.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the value type is a collection.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the value type is not registered.</exception>
        /// <exception cref="UnexpectedTypeException">Thrown if the key already exists with a different type and the mismatch behavior is set to throw.</exception>
        bool SetRaw(string key, object value, TypeMismatchBehaviour? overrideTypeMismatchBehaviour = null);

        /// <summary> Determines whether the specified key exists in the storage. </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        bool Has(string key);

        /// <summary> Gets the type of the value associated with the specified key. </summary>
        /// <param name="key">The key to get the type for.</param>
        /// <returns>The type of the value associated with the key, or null if the key does not exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        Type? TypeOf(string key);

        /// <summary> Determines whether the storage supports the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if the type is supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        bool Supports<T>();

        /// <summary> Gets the value associated with the specified key. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="defaultValue">The default value to use if the key does not exist.</param>
        /// <param name="overrideMissingKeyBehavior">Override default behavior when a requested key is not found in the storage.</param>
        /// <returns>The value associated with the key, the supplied default value, or the type's non-null default.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        /// <exception cref="UnexpectedTypeException">Thrown if the type of the value associated with the key does not match the expected type.</exception>
        T Get<T>(string key, T? defaultValue = default, MissingKeyBehavior? overrideMissingKeyBehavior = null);

        /// <summary> Sets the value for the specified key. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="overrideTypeMismatchBehaviour">Whether to override the value if the key already exists but with another type.</param>
        /// <returns>True if the value was set; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        /// <exception cref="UnexpectedTypeException">Thrown if the type of the value associated with the key does not match the expected type.</exception>
        bool Set<T>(string key, T value, TypeMismatchBehaviour? overrideTypeMismatchBehaviour = null) where T : notnull;

        /// <summary>
        /// Removes the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to remove the value for.</param>
        /// <returns>True if the key was removed; otherwise, false.</returns>
        bool Remove(string key);

        /// <summary>
        /// Removes values based on a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to determine which keys to remove.</param>
        /// <returns>The number of keys removed.</returns>
        int Remove(Func<string, bool> predicate);

        /// <summary>
        /// Removes all values from the storage.
        /// </summary>
        /// <returns>The number of keys removed.</returns>
        int RemoveAll();

        /// <summary> Saves the current data to disk. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        void Save();

        /// <summary> Begins a scope for making multiple changes. </summary>
        /// <returns>An IDisposable to end the scope.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        IDisposable MultipleChangeScope();

        /// <summary> Determines whether the storage supports lists of the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if lists of the type are supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        bool SupportsListsOf<T>();

        /// <summary> Determines whether the storage supports sets of the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if sets of the type are supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        bool SupportsSetsOf<T>();

        /// <summary> Determines whether the storage supports dictionaries of the specified key and value types. </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <returns>True if dictionaries of the key and value types are supported; otherwise, false.</returns>
        bool SupportsDictionariesOf<TKey, TValue>();

        /// <summary> Gets the list associated with the specified key. </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="key">The key to get the list for.</param>
        /// <returns>The list associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        IList<T> GetListOf<T>(string key);

        /// <summary> Gets the read-only list associated with the specified key. </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="key">The key to get the list for.</param>
        /// <returns>The read-only list associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        IReadOnlyList<T> GetReadOnlyListOf<T>(string key);

        /// <summary> Gets the set associated with the specified key. </summary>
        /// <typeparam name="T">The type of the set elements.</typeparam>
        /// <param name="key">The key to get the set for.</param>
        /// <returns>The set associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        ISet<T> GetSetOf<T>(string key);

        /// <summary> Gets the read-only set associated with the specified key. </summary>
        /// <typeparam name="T">The type of the set elements.</typeparam>
        /// <param name="key">The key to get the set for.</param>
        /// <returns>The read-only set associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        IReadOnlyCollection<T> GetReadOnlySetOf<T>(string key);

        /// <summary> Gets the dictionary associated with the specified key. </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <param name="key">The key to get the dictionary for.</param>
        /// <returns>The dictionary associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        IDictionary<TKey, TValue> GetDictionaryOf<TKey, TValue>(string key);

        /// <summary> Gets the read-only dictionary associated with the specified key. </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <param name="key">The key to get the dictionary for.</param>
        /// <returns>The read-only dictionary associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        IReadOnlyDictionary<TKey, TValue> GetReadOnlyDictionaryOf<TKey, TValue>(string key);
    }
}