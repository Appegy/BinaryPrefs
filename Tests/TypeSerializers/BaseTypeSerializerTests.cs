using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.BinaryStorage.TypeSerializers
{
    public class BaseTypeSerializerTests<TType, TBinaryType>
        where TBinaryType : TypeSerializer<TType>, new()
    {
        private readonly string _storagePath = Path.Combine(Application.temporaryCachePath, "test.bin");
        private readonly byte[] _buffer = new byte[4096];
        private readonly TType _value;
        private readonly TBinaryType _serializer;

        public BaseTypeSerializerTests(TType value)
        {
            _value = value;
            _serializer = new TBinaryType();
        }

        [SetUp, TearDown]
        public void CleanStorageBetweenTests()
        {
            if (File.Exists(_storagePath))
            {
                File.Delete(_storagePath);
            }
        }

        [Test]
        public void DoSizeCheckForTypeSerializer()
        {
            // Arrange
            var size = _serializer.SizeOf(_value);
            using var writeStream = new MemoryStream(_buffer);
            using var readStream = new MemoryStream(_buffer);
            using var writer = new BinaryWriter(writeStream);
            using var reader = new BinaryReader(readStream);

            // Act
            _serializer.WriteTo(writer, _value);
            var readValue = _serializer.ReadFrom(reader);

            // Assert
            writeStream.Position.Should().Be(size);
            readStream.Position.Should().Be(size);
            _serializer.Equals(_value, _value).Should().Be(true);
            _serializer.Equals(_value, readValue).Should().Be(true);
            readValue.Should().Be(_value);
        }

        [Test]
        public void DoGeneralStorageSectionCheck()
        {
            // Arrange
            using var storage = GetBuilderAt(_storagePath)
                .AddTypeSerializer(_serializer)
                .Build();

            // Act
            storage.Supports<TType>().Should().Be(true);
            storage.Set("key", _value);

            // Assert
            storage.TypeOf("key").Should().Be(typeof(TType));
            storage.Has("key").Should().Be(true);
            storage.Get<TType>("key").Should().Be(_value);

            storage.Remove("key").Should().Be(true);
            storage.Has("key").Should().Be(false);
            storage.Get<TType>("key").Should().Be(default(TType));
        }

        private BinaryPrefs.Builder GetBuilderAt(string storagePath)
        {
            var builder = BinaryPrefs.Construct(storagePath);
            return builder;
        }
    }
}