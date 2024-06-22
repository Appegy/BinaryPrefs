using System.IO;
using UnityEngine;

namespace Appegy.Storage
{
    public static class BinaryPrefs
    {
        private static readonly BinaryStorage _storage = BinaryStorage
            .Construct(Path.Combine(Application.persistentDataPath, PackageInfo.Name, "player_prefs.bin"))
            .AddPrimitiveTypes()
            .EnableAutoSaveOnChange()
            .SetMissingKeyBehaviour(MissingKeyBehavior.ReturnDefaultValueOnly)
            .SetTypeMismatchBehaviour(TypeMismatchBehaviour.OverrideValueAndType)
            .Build();

        /// <summary>
        /// Sets the value of the preference identified by the given key.
        /// </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetInt(string key, int value)
        {
            _storage.Set(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the current storage, it checks PlayerPrefs.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        /// <returns>The value corresponding to key.</returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            if (_storage.Has(key))
            {
                return _storage.Get(key, defaultValue);
            }

            if (PlayerPrefs.HasKey(key))
            {
                int value = PlayerPrefs.GetInt(key, defaultValue);
                _storage.Set(key, value);
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets the value of the preference identified by the given key.
        /// </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetFloat(string key, float value)
        {
            _storage.Set(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the current storage, it checks PlayerPrefs.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        /// <returns>The value corresponding to key.</returns>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            if (_storage.Has(key))
            {
                return _storage.Get(key, defaultValue);
            }

            if (PlayerPrefs.HasKey(key))
            {
                float value = PlayerPrefs.GetFloat(key, defaultValue);
                _storage.Set(key, value);
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Sets the value of the preference identified by the given key.
        /// </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetString(string key, string value)
        {
            _storage.Set(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the current storage, it checks PlayerPrefs.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        /// <returns>The value corresponding to key.</returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (_storage.Has(key))
            {
                return _storage.Get(key, defaultValue);
            }

            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key, defaultValue);
                _storage.Set(key, value);
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Returns true if the key exists in the preference file.
        /// </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public static bool HasKey(string key)
        {
            return _storage.Has(key) || PlayerPrefs.HasKey(key);
        }

        /// <summary>
        /// Removes the given key from the preference file.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public static void DeleteKey(string key)
        {
            _storage.Remove(key);
        }

        /// <summary>
        /// Removes all keys and values from the preference file.
        /// </summary>
        public static void DeleteAll()
        {
            _storage.RemoveAll();
        }

        /// <summary>
        /// Writes all modified preferences to disk.
        /// </summary>
        public static void Save()
        {
            _storage.Save();
        }
    }
}