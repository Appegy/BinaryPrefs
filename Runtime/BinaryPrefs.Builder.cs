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
            private readonly List<TypedBinarySection> _serializers = new();
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