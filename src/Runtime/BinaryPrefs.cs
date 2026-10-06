using System;
using System.IO;
using UnityEngine;

namespace Appegy.Storage
{
    /// <summary>
    /// A drop-in replacement for <see cref="PlayerPrefs"/> backed by <see cref="BinaryStorage"/>.
    /// Values missing from the binary storage are read from <see cref="PlayerPrefs"/> once, moved into the
    /// binary storage and removed from <see cref="PlayerPrefs"/>, so every key converges to a single source of truth.
    /// </summary>
    public static class BinaryPrefs
    {
        private const string StorageFileName = "player_prefs.bin";

        private const int IntProbeA = int.MinValue;
        private const int IntProbeB = int.MaxValue;
        private const float FloatProbeA = float.MinValue;
        private const float FloatProbeB = float.MaxValue;
        private const string StringProbeA = "appegy.binary-prefs.probe.a";
        private const string StringProbeB = "appegy.binary-prefs.probe.b";

        private static BinaryStorage _storage;
        private static string _storageFilePath;

        private static string StorageFilePath => _storageFilePath ??= Path.Combine(PackageInfo.PersistentFolder, StorageFileName);

        private static BinaryStorage Storage => _storage ??= BinaryStorage
            .Construct(StorageFilePath)
            .AddPrimitiveTypes()
            .EnableAutoSaveOnChange()
            .SetMissingKeyBehaviour(MissingKeyBehavior.ReturnDefaultValueOnly)
            .SetTypeMismatchBehaviour(TypeMismatchBehaviour.OverrideValueAndType)
            .Build();

        #region Int

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetInt(string key, int value)
        {
            Write(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the binary storage, it is looked up in <see cref="PlayerPrefs"/> and migrated.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist or was stored with another type.</param>
        /// <returns>The value corresponding to key.</returns>
        public static int GetInt(string key, int defaultValue = 0)
        {
            if (TryReadStored(key, out int stored))
            {
                return stored;
            }
            if (TryReadLegacyInt(key, out var legacy))
            {
                return Migrate(key, legacy);
            }
            return defaultValue;
        }

        #endregion

        #region Float

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetFloat(string key, float value)
        {
            Write(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the binary storage, it is looked up in <see cref="PlayerPrefs"/> and migrated.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist or was stored with another type.</param>
        /// <returns>The value corresponding to key.</returns>
        public static float GetFloat(string key, float defaultValue = 0f)
        {
            if (TryReadStored(key, out float stored))
            {
                return stored;
            }
            if (TryReadLegacyFloat(key, out var legacy))
            {
                return Migrate(key, legacy);
            }
            return defaultValue;
        }

        #endregion

        #region String

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetString(string key, string value)
        {
            Write(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the binary storage, it is looked up in <see cref="PlayerPrefs"/> and migrated.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist or was stored with another type.</param>
        /// <returns>The value corresponding to key.</returns>
        public static string GetString(string key, string defaultValue = "")
        {
            if (TryReadStored(key, out string stored))
            {
                return stored;
            }
            if (TryReadLegacyString(key, out var legacy))
            {
                return Migrate(key, legacy);
            }
            return defaultValue;
        }

        #endregion

        #region Bool

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetBool(string key, bool value)
        {
            Write(key, value);
        }

        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// If the key is not found in the binary storage, it is looked up in <see cref="PlayerPrefs"/> as an int
        /// (the conventional way of storing booleans there) and migrated. Only 0 and 1 are treated as booleans,
        /// so an int preference holding any other value is neither migrated nor removed.
        /// </summary>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist or was stored with another type.</param>
        /// <returns>The value corresponding to key.</returns>
        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (TryReadStored(key, out bool stored))
            {
                return stored;
            }
            if (TryReadLegacyInt(key, out var legacy) && legacy is 0 or 1)
            {
                return Migrate(key, legacy == 1);
            }
            return defaultValue;
        }

        #endregion

        #region Extended types

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetLong(string key, long value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static long GetLong(string key, long defaultValue = 0L) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetDouble(string key, double value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static double GetDouble(string key, double defaultValue = 0d) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetDateTime(string key, DateTime value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static DateTime GetDateTime(string key, DateTime defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetTimeSpan(string key, TimeSpan value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static TimeSpan GetTimeSpan(string key, TimeSpan defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetVector2(string key, Vector2 value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Vector2 GetVector2(string key, Vector2 defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetVector3(string key, Vector3 value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Vector3 GetVector3(string key, Vector3 defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetVector4(string key, Vector4 value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Vector4 GetVector4(string key, Vector4 defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetVector2Int(string key, Vector2Int value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Vector2Int GetVector2Int(string key, Vector2Int defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetVector3Int(string key, Vector3Int value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Vector3Int GetVector3Int(string key, Vector3Int defaultValue = default) => Read(key, defaultValue);

        /// <summary> Sets the value of the preference identified by the given key. </summary>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetQuaternion(string key, Quaternion value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        public static Quaternion GetQuaternion(string key, Quaternion defaultValue = default) => Read(key, defaultValue);

        /// <summary>
        /// Sets the value of the preference identified by the given key.
        /// The enum is stored as its underlying integral value, so no per-enum registration is required.
        /// </summary>
        /// <typeparam name="T">The enum type to store.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        public static void SetEnum<T>(string key, T value)
            where T : unmanaged, Enum
        {
            Write(key, ToRawEnumValue(value));
        }

        /// <summary> Returns the enum value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        /// <typeparam name="T">The enum type to read.</typeparam>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        public static T GetEnum<T>(string key, T defaultValue = default)
            where T : unmanaged, Enum
        {
            return TryReadStored(key, out long stored) ? (T)Enum.ToObject(typeof(T), stored) : defaultValue;
        }

        /// <summary>
        /// Sets the value of the preference identified by the given key.
        /// The type must be registered in the storage; all types added by <c>AddPrimitiveTypes</c> are supported.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not supported by the storage.</exception>
        public static void Set<T>(string key, T value) => Write(key, value);

        /// <summary> Returns the value corresponding to key, or <paramref name="defaultValue"/> if it is missing or was stored with another type. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to retrieve the value for.</param>
        /// <param name="defaultValue">The default value to return if the key does not exist.</param>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not supported by the storage.</exception>
        public static T Get<T>(string key, T defaultValue = default) => Read(key, defaultValue);

        /// <summary> Returns the type the key is stored with, or null if the key is not present in the binary storage. </summary>
        /// <param name="key">The key to get the type for.</param>
        public static Type TypeOf(string key) => Storage.TypeOf(key);

        #endregion

        #region Management

        /// <summary> Returns true if the key exists in the preference file or in the not yet migrated <see cref="PlayerPrefs"/>. </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        public static bool HasKey(string key)
        {
            return Storage.Has(key) || PlayerPrefs.HasKey(key);
        }

        /// <summary> Removes the given key from the preference file and from <see cref="PlayerPrefs"/>. </summary>
        /// <param name="key">The key to remove.</param>
        public static void DeleteKey(string key)
        {
            Storage.Remove(key);
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Removes all keys and values from the preference file and from <see cref="PlayerPrefs"/>.
        /// Mirrors <see cref="PlayerPrefs.DeleteAll"/>, which wipes every key of the application, including keys written by Unity and third-party packages.
        /// </summary>
        public static void DeleteAll()
        {
            Storage.RemoveAll();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        /// <summary> Writes all modified preferences to disk. </summary>
        public static void Save()
        {
            Storage.Save();
        }

        #endregion

        #region Internals

        internal static void OverrideStorageFilePath(string filePath)
        {
            DisposeStorage();
            _storageFilePath = filePath;
        }

        internal static void Reset()
        {
            DisposeStorage();
            _storageFilePath = null;
        }

        private static void DisposeStorage()
        {
            _storage?.Dispose();
            _storage = null;
        }

        private static void Write<T>(string key, T value)
        {
            var exists = Storage.Has(key);
            Storage.Set(key, value);
            if (!exists && PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        private static T Read<T>(string key, T defaultValue)
        {
            return TryReadStored(key, out T stored) ? stored : defaultValue;
        }

        private static bool TryReadStored<T>(string key, out T value)
        {
            if (Storage.TypeOf(key) == typeof(T))
            {
                value = Storage.Get<T>(key);
                return true;
            }
            value = default;
            return false;
        }

        private static T Migrate<T>(string key, T value)
        {
            Storage.Set(key, value);
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            return value;
        }

        private static bool TryReadLegacyInt(string key, out int value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                value = default;
                return false;
            }
            value = PlayerPrefs.GetInt(key, IntProbeA);
            if (value != IntProbeA)
            {
                return true;
            }
            value = PlayerPrefs.GetInt(key, IntProbeB);
            return value != IntProbeB;
        }

        private static bool TryReadLegacyFloat(string key, out float value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                value = default;
                return false;
            }
            value = PlayerPrefs.GetFloat(key, FloatProbeA);
            if (value != FloatProbeA)
            {
                return true;
            }
            value = PlayerPrefs.GetFloat(key, FloatProbeB);
            return value != FloatProbeB;
        }

        private static bool TryReadLegacyString(string key, out string value)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                value = default;
                return false;
            }
            value = PlayerPrefs.GetString(key, StringProbeA);
            if (value != StringProbeA)
            {
                return true;
            }
            value = PlayerPrefs.GetString(key, StringProbeB);
            return value != StringProbeB;
        }

        private static long ToRawEnumValue<T>(T value)
            where T : unmanaged, Enum
        {
            return Enum.GetUnderlyingType(typeof(T)) == typeof(ulong)
                ? unchecked((long)Convert.ToUInt64(value))
                : Convert.ToInt64(value);
        }

        #endregion
    }
}
