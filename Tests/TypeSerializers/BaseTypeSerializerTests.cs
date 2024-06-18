using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    public class BaseTypeSerializerTests<TType, TTypeSerializer> : TypeSerializerTests<TType, TTypeSerializer>
        where TTypeSerializer : TypeSerializer<TType>, new()
    {
        protected BaseTypeSerializerTests(TType defaultValue) : base(defaultValue, new TTypeSerializer())
        {
        }
    }

    public class TypeSerializerTests<TType, TTypeSerializer> : BaseStorageTests
        where TTypeSerializer : TypeSerializer<TType>
    {
        private readonly byte[] _buffer = new byte[4096];
        private readonly TType _defaultValue;
        private readonly TTypeSerializer _serializer;

        public TypeSerializerTests(TType defaultValue, TTypeSerializer serializer)
        {
            _defaultValue = defaultValue;
            _serializer = serializer;
        }

        [Test]
        public void General_Checks_For_Type_Serializer()
        {
            // Arrange
            using var writeStream = new MemoryStream(_buffer);
            using var readStream = new MemoryStream(_buffer);
            using var writer = new BinaryWriter(writeStream);
            using var reader = new BinaryReader(readStream);

            // Act
            _serializer.WriteTo(writer, _defaultValue);
            var readValue = _serializer.ReadFrom(reader);

            // Assert
            writeStream.Position.Should().Be(readStream.Position, "Write stream position should match the read stream position");
            readValue.Should().Be(_defaultValue, "The deserialized value should be equal to the default value");
            _serializer.Equals(_defaultValue, _defaultValue).Should().Be(true, "The default value should be equal to itself using serializer's Equals");
            _serializer.Equals(_defaultValue, readValue).Should().Be(true, "The default value should be equal to the deserialized value using serializer's Equals");
        }

        [Test]
        public void General_Checks_For_Type_Storage()
        {
            // Arrange
            using var storage = BinaryStorage
                .Construct(StoragePath)
                .AddTypeSerializer(_serializer)
                .Build();

            // Act
            var added = storage.Set("key", _defaultValue);

            // Assert
            added.Should().Be(true, "The value should have been added to storage");

            storage.Supports<TType>().Should().Be(true, "Storage must support the type {0}", typeof(TType).Name);
            storage.Has("key").Should().Be(true, "Storage must contain the added key");
            storage.TypeOf("key").Should().Be(typeof(TType), "The key type must be {0}", typeof(TType).Name);
            storage.Get<TType>("key").Should().Be(_defaultValue, "The value must be equal to the just added value");

            storage.Remove("key").Should().Be(true, "The key should be successfully removed from the storage.");
            storage.Has("key").Should().Be(false, "Storage must not contain the removed key");
            storage.Get<TType>("key").Should().Be(default(TType), "The value retrieved after removal must be default(TType)");
        }
    }
}