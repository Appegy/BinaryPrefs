using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using Appegy.Storage;

namespace Appegy.Tests.Storage
{
    [TestFixture]
    public class BinaryPrefsIntTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear PlayerPrefs and BinaryPrefs before each test
            PlayerPrefs.DeleteAll();
            BinaryPrefs.DeleteAll();
        }

        [Test]
        public void SetInt_ShouldStoreValueInBinaryStorage()
        {
            // Arrange
            var key = "testInt";
            var value = 42;

            // Act
            BinaryPrefs.SetInt(key, value);

            // Assert
            BinaryPrefs.GetInt(key).Should().Be(value);
        }

        [Test]
        public void GetInt_ShouldReturnDefaultValueIfKeyNotFound()
        {
            // Arrange
            var key = "unknownInt";
            var defaultValue = 10;

            // Act
            var result = BinaryPrefs.GetInt(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void GetInt_ShouldReturnValueFromPlayerPrefsIfNotInBinaryStorage()
        {
            // Arrange
            var key = "testInt";
            var playerPrefsValue = 100;

            // Store value in PlayerPrefs only
            PlayerPrefs.SetInt(key, playerPrefsValue);

            // Act
            var result = BinaryPrefs.GetInt(key);

            // Assert
            result.Should().Be(playerPrefsValue);

            // Verify that the value is now stored in BinaryStorage
            BinaryPrefs.GetInt(key).Should().Be(playerPrefsValue);
        }

        [Test]
        public void GetInt_ShouldReturnDefaultValueIfKeyNotFoundInBothStorages()
        {
            // Arrange
            var key = "nonExistentKey";
            var defaultValue = 20;

            // Act
            var result = BinaryPrefs.GetInt(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void SetInt_ShouldOverrideExistingValueInBinaryStorage()
        {
            // Arrange
            var key = "testInt";
            var initialValue = 42;
            var newValue = 84;

            // Store initial value
            BinaryPrefs.SetInt(key, initialValue);

            // Act
            BinaryPrefs.SetInt(key, newValue);

            // Assert
            BinaryPrefs.GetInt(key).Should().Be(newValue);
        }
    }
}
