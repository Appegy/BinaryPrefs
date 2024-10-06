using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
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
            using (var storage = BinaryStorage
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                using (storage.MultipleChangeScope())
                {
                    storage.Set("key_s", "value");
                    storage.Set("key_b", true);
                    storage.Set("key_l", 60L);
                }
            }

            // Act
            // Assert
            using (var storage = BinaryStorage
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
            using (var storage = BinaryStorage
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                using (storage.MultipleChangeScope())
                {
                    storage.Set("key_s", "value");
                    storage.Set("key_b", true);
                    storage.Set("key_l", 60L);
                }
            }

            // Act
            // Assert
            using (var storage = BinaryStorage
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

        [Test]
        public void WhenStorageHasObsoleteSection_AndStorageLoaded_ThenStorageExceptionThrown()
        {
            // Arrange
            using (var storage = BinaryStorage
                       .Construct(_storagePath)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .AddTypeSerializer(BooleanSerializer.Shared)
                       .AddTypeSerializer(Int64Serializer.Shared)
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                using (storage.MultipleChangeScope())
                {
                    storage.Set("key_s", "value");
                    storage.Set("key_b", true);
                    storage.Set("key_l", 60L);
                }
            }

            // Act
            Action action = () =>
            {
                using var storage = BinaryStorage
                    .Construct(_storagePath)
                    .AddTypeSerializer(BooleanSerializer.Shared)
                    .AddTypeSerializer(Int64Serializer.Shared)
                    .Build(KeyLoadFailedBehaviour.ThrowException);
            };

            // Assert
            action.Should().Throw<KeyLoadFailedException>();
        }

        [Test]
        public void WhenStorageSupportsTwoEnumsWithSameName_AndNamespacesAreDifferent_ThenDataIsNotCorrupting()
        {
            // Arrange
            using (var storage = BinaryStorage
                       .Construct(_storagePath)
                       .EnableAutoSaveOnChange()
                       .SupportEnum<ScreenOrientation>()
                       .SupportEnum<UnityEngine.ScreenOrientation>(true)
                       .Build())
            {
                using (storage.MultipleChangeScope())
                {
                    storage.Set("key_1", UnityEngine.ScreenOrientation.LandscapeLeft);
                    storage.Set("key_2", ScreenOrientation.LandscapeLeft);
                    storage.Set("key_3", ScreenOrientation.LandscapeRight);
                }
            }

            // Act
            // Assert
            using (var storage = BinaryStorage
                       .Construct(_storagePath)
                       .SupportEnum<ScreenOrientation>()
                       .SupportEnum<UnityEngine.ScreenOrientation>(true)
                       .Build())
            {
                storage.Get<UnityEngine.ScreenOrientation>("key_1").Should().Be(UnityEngine.ScreenOrientation.LandscapeLeft);
                storage.Get<ScreenOrientation>("key_2").Should().Be(ScreenOrientation.LandscapeLeft);
                storage.Get<ScreenOrientation>("key_3").Should().Be(ScreenOrientation.LandscapeRight);
            }
        }

        public enum ScreenOrientation : byte
        {
            LandscapeLeft,
            LandscapeRight,
        }
    }
}