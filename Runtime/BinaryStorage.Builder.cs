using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Appegy.Storage
{
    public partial class BinaryStorage
    {
        static partial void LockFilePathInEditor(string filePath);
        static partial void ThrowIfFilePathLocked(string filePath);
        static partial void UnlockFilePathInEditor(string filePath);

        /// <summary> Creates and configures a new instance of <see cref="BinaryStorage"/> with default settings. </summary>
        /// <param name="filePath">The file path for the storage.</param>
        /// <returns>A configured <see cref="BinaryStorage"/> instance.</returns>
        public static BinaryStorage Get(string filePath)
        {
            return Construct(filePath)
                .AddPrimitiveTypes()
                .EnableAutoSaveOnChange()
                .Build();
        }

        /// <summary> Begins the construction of a new <see cref="BinaryStorage"/> instance. </summary>
        /// <param name="filePath">The file path for the storage.</param>
        /// <returns>A <see cref="Builder"/> for configuring the <see cref="BinaryStorage"/> instance.</returns>
        public static Builder Construct(string filePath)
        {
            ThrowIfFilePathLocked(filePath);
            LockFilePathInEditor(filePath);
            return new Builder(filePath);
        }

        /// <summary> Deletes the storage file at the specified path. </summary>
        /// <param name="storagePath">The path to the storage file.</param>
        internal static void Delete(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }

        /// <summary> Provides a fluent interface for configuring and building a <see cref="BinaryStorage"/> instance. </summary>
        public class Builder
        {
            private readonly string _filePath;
            private readonly List<BinarySection> _serializers = new();
            private bool _autoSave;
            private MissingKeyBehavior _missingKeyBehavior = MissingKeyBehavior.InitializeWithDefaultValue;
            private TypeMismatchBehaviour _typeMismatchBehaviour = TypeMismatchBehaviour.ThrowException;

            internal Builder(string filePath)
            {
                _filePath = filePath;
            }

            /// <summary> Enables automatic saving of changes to the storage. </summary>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            public Builder EnableAutoSaveOnChange()
            {
                _autoSave = true;
                return this;
            }

            /// <summary> Specifies the behavior when a requested key is not found in the storage. </summary>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            public Builder SetMissingKeyBehaviour(MissingKeyBehavior behavior)
            {
                _missingKeyBehavior = behavior;
                return this;
            }
            /// <summary> Specifies the behavior when the type of value associated with a key does not match the expected type. </summary>
            /// <param name="behavior">The type mismatch behavior.</param>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            public Builder SetTypeMismatchBehaviour(TypeMismatchBehaviour behavior)
            {
                _typeMismatchBehaviour = behavior;
                return this;
            }

            /// <summary> Adds serializers for primitive types to the storage configuration. </summary>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            public Builder AddPrimitiveTypes()
            {
                return AddTypeSerializer(BooleanSerializer.Shared)
                    .AddTypeSerializer(CharSerializer.Shared)
                    .AddTypeSerializer(ByteSerializer.Shared)
                    .AddTypeSerializer(SByteSerializer.Shared)
                    .AddTypeSerializer(Int16Serializer.Shared)
                    .AddTypeSerializer(UInt16Serializer.Shared)
                    .AddTypeSerializer(Int32Serializer.Shared)
                    .AddTypeSerializer(UInt32Serializer.Shared)
                    .AddTypeSerializer(Int64Serializer.Shared)
                    .AddTypeSerializer(UInt64Serializer.Shared)
                    .AddTypeSerializer(SingleSerializer.Shared)
                    .AddTypeSerializer(DoubleSerializer.Shared)
                    .AddTypeSerializer(DecimalSerializer.Shared)
                    .AddTypeSerializer(StringSerializer.Shared)
                    .AddTypeSerializer(DateTimeSerializer.Shared)
                    .AddTypeSerializer(TimeSpanSerializer.Shared)
                    .AddTypeSerializer(QuaternionSerializer.Shared)
                    .AddTypeSerializer(Vector2Serializer.Shared)
                    .AddTypeSerializer(Vector3Serializer.Shared)
                    .AddTypeSerializer(Vector4Serializer.Shared)
                    .AddTypeSerializer(Vector2IntSerializer.Shared)
                    .AddTypeSerializer(Vector3IntSerializer.Shared);
            }

            /// <summary> Adds a serializer for a specified type to the storage configuration. </summary>
            /// <typeparam name="T">The type to be serialized.</typeparam>
            /// <param name="typeSerializer">The serializer for the specified type.</param>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            /// <exception cref="DuplicateTypeSerializerException">Thrown if a serializer for the specified type already exists.</exception>
            public Builder AddTypeSerializer<T>(TypeSerializer<T> typeSerializer)
            {
                if (_serializers.Any(c => c is TypedBinarySection<T>))
                {
                    throw new DuplicateTypeSerializerException(typeof(T), typeSerializer.TypeName, _filePath);
                }
                _serializers.Add(new TypedBinarySection<T>(typeSerializer));
                return this;
            }

            /// <summary> Adds support for a specified enum type to the storage configuration. </summary>
            /// <typeparam name="T">The enum type to be supported.</typeparam>
            /// <param name="useFullName">Whether to use the full name of the enum type.</param>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            /// <exception cref="UnexpectedUnderlyingEnumTypeException">Thrown if the enum has an unexpected underlying type.</exception>
            public Builder SupportEnum<T>(bool useFullName = false)
                where T : unmanaged, Enum
            {
                var enumType = typeof(T);
                var underlyingType = Enum.GetUnderlyingType(enumType);
                switch (underlyingType)
                {
                    case not null when underlyingType == typeof(byte):
                        AddTypeSerializer(new EnumTypeSerializer<T, byte>(ByteSerializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(sbyte):
                        AddTypeSerializer(new EnumTypeSerializer<T, sbyte>(SByteSerializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(short):
                        AddTypeSerializer(new EnumTypeSerializer<T, short>(Int16Serializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(ushort):
                        AddTypeSerializer(new EnumTypeSerializer<T, ushort>(UInt16Serializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(int):
                        AddTypeSerializer(new EnumTypeSerializer<T, int>(Int32Serializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(uint):
                        AddTypeSerializer(new EnumTypeSerializer<T, uint>(UInt32Serializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(long):
                        AddTypeSerializer(new EnumTypeSerializer<T, long>(Int64Serializer.Shared, useFullName));
                        break;
                    case not null when underlyingType == typeof(ulong):
                        AddTypeSerializer(new EnumTypeSerializer<T, ulong>(UInt64Serializer.Shared, useFullName));
                        break;
                    default:
                        throw new UnexpectedUnderlyingEnumTypeException(enumType, underlyingType);
                }
                return this;
            }

            /// <summary> Adds support for lists of a specified type to the storage configuration. </summary>
            /// <typeparam name="T">The type of elements in the list.</typeparam>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            /// <exception cref="CantSupportCollectionOfException">Thrown if the specified type is not supported.</exception>
            public Builder SupportListsOf<T>()
            {
                if (_serializers.FirstOrDefault(c => c is TypedBinarySection<T>) is not TypedBinarySection<T> section)
                {
                    throw new CantSupportCollectionOfException(typeof(T));
                }

                return AddTypeSerializer(new CollectionTypeSerializer<T, ReactiveList<T>>(section.Serializer));
            }

            /// <summary> Adds support for sets of a specified type to the storage configuration. </summary>
            /// <typeparam name="T">The type of elements in the set.</typeparam>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            /// <exception cref="CantSupportCollectionOfException">Thrown if the specified type is not supported.</exception>
            public Builder SupportSetsOf<T>()
            {
                if (_serializers.FirstOrDefault(c => c is TypedBinarySection<T>) is not TypedBinarySection<T> section)
                {
                    throw new CantSupportCollectionOfException(typeof(T));
                }

                return AddTypeSerializer(new CollectionTypeSerializer<T, ReactiveSet<T>>(section.Serializer));
            }

            /// <summary> Adds support for dictionaries of specified key and value types to the storage configuration. </summary>
            /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
            /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
            /// <returns>The current <see cref="Builder"/> instance for method chaining.</returns>
            /// <exception cref="CantSupportCollectionOfException">Thrown if the specified key or value type is not supported.</exception>
            public Builder SupportDictionariesOf<TKey, TValue>()
            {
                if (_serializers.FirstOrDefault(c => c is TypedBinarySection<TKey>) is not TypedBinarySection<TKey> keySection ||
                    _serializers.FirstOrDefault(c => c is TypedBinarySection<TValue>) is not TypedBinarySection<TValue> valueSection)
                {
                    throw new CantSupportCollectionOfException(typeof(KeyValuePair<TKey, TValue>));
                }

                var kvSerializer = new KeyValueTypeSerializer<TKey, TValue>(keySection.Serializer, valueSection.Serializer);
                return AddTypeSerializer(new CollectionTypeSerializer<KeyValuePair<TKey, TValue>, ReactiveDictionary<TKey, TValue>>(kvSerializer));
            }

            /// <summary> Builds and returns the configured <see cref="BinaryStorage"/> instance. </summary>
            /// <returns>The configured <see cref="BinaryStorage"/> instance.</returns>
            /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
            /// <exception cref="IOException"> An I/O error occurred </exception>
            public BinaryStorage Build()
            {
                try
                {
                    var storage = new BinaryStorage(_filePath, _serializers);
                    storage.AutoSave = _autoSave;
                    storage.MissingKeyBehavior = _missingKeyBehavior;
                    storage.TypeMismatchBehaviour = _typeMismatchBehaviour;
                    storage.LoadDataFromDisk();
                    return storage;
                }
                catch
                {
                    UnlockFilePathInEditor(_filePath);
                    throw;
                }
            }
        }
    }
}