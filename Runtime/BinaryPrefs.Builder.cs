using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Appegy.BinaryStorage
{
    public partial class BinaryPrefs
    {
        static partial void LockFilePathInEditor(string filePath);
        static partial void ThrowIfFilePathLocked(string filePath);
        static partial void UnlockFilePathInEditor(string filePath);

        public static BinaryPrefs Get(string filePath)
        {
            return Construct(filePath)
                .AddPrimitiveTypes()
                .EnableAutoSaveOnChange()
                .Build();
        }

        public static Builder Construct(string filePath)
        {
            ThrowIfFilePathLocked(filePath);
            LockFilePathInEditor(filePath);
            return new Builder(filePath);
        }

        internal static void Delete(string storagePath)
        {
            if (File.Exists(storagePath))
            {
                File.Delete(storagePath);
            }
        }

        public class Builder
        {
            private readonly string _filePath;
            private readonly List<BinarySection> _serializers = new();
            private bool _autoSave;

            public Builder(string filePath)
            {
                _filePath = filePath;
            }

            public Builder EnableAutoSaveOnChange()
            {
                _autoSave = true;
                return this;
            }

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

            public Builder AddTypeSerializer<T>(TypeSerializer<T> typeSerializer)
            {
                if (_serializers.Any(c => c is TypedBinarySection<T>))
                {
                    throw new DuplicateTypeSerializerException(typeof(T), typeSerializer.TypeName, _filePath);
                }
                _serializers.Add(new TypedBinarySection<T>(typeSerializer));
                return this;
            }

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

            public Builder SupportListsOf<T>()
            {
                if (_serializers.FirstOrDefault(c => c is TypedBinarySection<T>) is not TypedBinarySection<T> typeSerializer)
                {
                    throw new CantSupportListOfException(typeof(T));
                }

                return AddTypeSerializer(new CollectionTypeSerializer<T, ReactiveList<T>>(typeSerializer.Serializer));
            }

            public Builder SupportSetsOf<T>()
            {
                if (_serializers.FirstOrDefault(c => c is TypedBinarySection<T>) is not TypedBinarySection<T> typeSerializer)
                {
                    throw new CantSupportListOfException(typeof(T));
                }

                return AddTypeSerializer(new CollectionTypeSerializer<T, ReactiveHashSet<T>>(typeSerializer.Serializer));
            }

            public BinaryPrefs Build()
            {
                try
                {
                    var storage = new BinaryPrefs(_filePath, _serializers);
                    storage.AutoSave = _autoSave;
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