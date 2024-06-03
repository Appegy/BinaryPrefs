using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.BinaryStorage
{
    public class BinaryStorageTests
    {
        private readonly string _storagePath = Path.Combine(Application.temporaryCachePath, "test.bin");

        [SetUp, TearDown]
        public void CleanStorageBetweenTests()
        {
            if (File.Exists(_storagePath))
            {
                File.Delete(_storagePath);
            }
        }

        [Test]
        public void WhenStorageCreated_AndPrimitiveTypesAdded_ThenAllStandardTypesSupported()
        {
            // Arrange
            using var storage = BinaryPrefs.Construct(_storagePath)
                .AddPrimitiveTypes()
                .Build();

            // Assert
            storage.Supports<bool>().Should().Be(true);
            storage.Supports<char>().Should().Be(true);
            storage.Supports<byte>().Should().Be(true);
            storage.Supports<sbyte>().Should().Be(true);
            storage.Supports<short>().Should().Be(true);
            storage.Supports<ushort>().Should().Be(true);
            storage.Supports<uint>().Should().Be(true);
            storage.Supports<int>().Should().Be(true);
            storage.Supports<long>().Should().Be(true);
            storage.Supports<ulong>().Should().Be(true);
            storage.Supports<float>().Should().Be(true);
            storage.Supports<double>().Should().Be(true);
            storage.Supports<decimal>().Should().Be(true);
            storage.Supports<string>().Should().Be(true);
            storage.Supports<DateTime>().Should().Be(true);
            storage.Supports<TimeSpan>().Should().Be(true);
            storage.Supports<Quaternion>().Should().Be(true);
            storage.Supports<Vector2>().Should().Be(true);
            storage.Supports<Vector3>().Should().Be(true);
            storage.Supports<Vector4>().Should().Be(true);
            storage.Supports<Vector2Int>().Should().Be(true);
            storage.Supports<Vector3Int>().Should().Be(true);
        }

        [Test]
        public void WhenStorageHasFewKeys_AndResetAllCalled_ThenAllDataHasBeenErased()
        {
            // Arrange
            using var storage = BinaryPrefs.Construct(_storagePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .AddTypeSerializer(StringSerializer.Shared)
                .Build();

            storage.Set("key_i", 10);
            storage.Set("key_s", "value");

            // Act
            storage.RemoveAll();

            // Assert
            storage.Has("key_i").Should().Be(false);
            storage.Has("key_s").Should().Be(false);
        }

        [Test]
        public void WhenStorageHasFewKeys_AndResetAllWithPredicateCalled_ThenRemoveOnlyPredictedKeys()
        {
            // Arrange
            using var storage = BinaryPrefs.Construct(_storagePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            storage.Set("prefix1.key1", 11);
            storage.Set("prefix1.key2", 12);
            storage.Set("prefix2.key1", 21);
            storage.Set("prefix2.key2", 22);

            // Act
            storage.Remove(key => key.StartsWith("prefix1"));

            // Assert
            storage.Has("prefix1.key1").Should().Be(false);
            storage.Has("prefix1.key2").Should().Be(false);
            storage.Has("prefix2.key1").Should().Be(true);
            storage.Has("prefix2.key2").Should().Be(true);
        }
    }
}