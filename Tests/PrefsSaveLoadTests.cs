using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.BinaryStorage
{
    public class PrefsSaveLoadTests
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
        public void WhenStorageChanged_AndReloaded_ThenAllDataAreValid()
        {
            // Arrange
            using (var storage = BinaryPrefs
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .Build())
            {
                storage.Set("key_s", "value");
                storage.Set("key_b", true);
                storage.Set("key_l", 60L);
                storage.Save();
            }

            // Act
            // Assert
            using (var storage = BinaryPrefs
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .Build())
            {
                storage.Has("key_s").Should().BeTrue();
                storage.Has("key_b").Should().BeTrue();
                storage.Has("key_l").Should().BeTrue();

                storage.Get<string>("key_s").Should().Be("value");
                storage.Get<bool>("key_b").Should().Be(true);
                storage.Get<long>("key_l").Should().Be(60L);
            }
        }

        [Test]
        public void WhenStorageHasObsoleteSection_AndStorageLoaded_ThenObsoleteSectionIgnored()
        {
            // Arrange
            using (var storage = BinaryPrefs
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                storage.Set("key_s", "value");
                storage.Set("key_b", true);
                storage.Set("key_l", 60L);
            }

            // Act
            // Assert
            using (var storage = BinaryPrefs
                       .Construct(_storagePath)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .Build())
            {
                storage.Supports<string>().Should().BeFalse();
                storage.Has("key_s").Should().BeFalse();
                storage.Has("key_b").Should().BeTrue();
                storage.Has("key_l").Should().BeTrue();
            }
        }
    }
}