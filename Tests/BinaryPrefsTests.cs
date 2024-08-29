using NUnit.Framework;
using FluentAssertions;
using UnityEngine;
using Appegy.Storage;

namespace Appegy.Tests.Storage
{
    [TestFixture]
    public class BinaryPrefsIntTests
    {
        [SetUp, TearDown]
        public void SetUp()
        {
            // Clear PlayerPrefs and BinaryPrefs before each test
            PlayerPrefs.DeleteAll();
            BinaryPrefs.DeleteAll();
        }

        #region Integer

        [Test]
        public void SetInt_ShouldStoreValueInBinaryStorage()
        {
            // Arrange
            var key = "key";
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
            var key = "unknownKey";
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
            var key = "key";
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
            var key = "key";
            var initialValue = 42;
            var newValue = 84;

            // Store initial value
            BinaryPrefs.SetInt(key, initialValue);

            // Act
            BinaryPrefs.SetInt(key, newValue);

            // Assert
            BinaryPrefs.GetInt(key).Should().Be(newValue);
        }

        #endregion

        #region Float

        [Test]
        public void SetFloat_ShouldStoreValueInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var value = 42f;

            // Act
            BinaryPrefs.SetFloat(key, value);

            // Assert
            BinaryPrefs.GetFloat(key).Should().Be(value);
        }

        [Test]
        public void GetFloat_ShouldReturnDefaultValueIfKeyNotFound()
        {
            // Arrange
            var key = "unknownKey";
            var defaultValue = 10f;

            // Act
            var result = BinaryPrefs.GetFloat(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void GetFloat_ShouldReturnValueFromPlayerPrefsIfNotInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var playerPrefsValue = 100f;

            // Store value in PlayerPrefs only
            PlayerPrefs.SetFloat(key, playerPrefsValue);

            // Act
            var result = BinaryPrefs.GetFloat(key);

            // Assert
            result.Should().Be(playerPrefsValue);

            // Verify that the value is now stored in BinaryStorage
            BinaryPrefs.GetFloat(key).Should().Be(playerPrefsValue);
        }

        [Test]
        public void GetFloat_ShouldReturnDefaultValueIfKeyNotFoundInBothStorages()
        {
            // Arrange
            var key = "nonExistentKey";
            var defaultValue = 20f;

            // Act
            var result = BinaryPrefs.GetFloat(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void SetFloat_ShouldOverrideExistingValueInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var initialValue = 42f;
            var newValue = 84f;

            // Store initial value
            BinaryPrefs.SetFloat(key, initialValue);

            // Act
            BinaryPrefs.SetFloat(key, newValue);

            // Assert
            BinaryPrefs.GetFloat(key).Should().Be(newValue);
        }

        #endregion

        #region String

        [Test]
        public void SetString_ShouldStoreValueInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var value = "42";

            // Act
            BinaryPrefs.SetString(key, value);

            // Assert
            BinaryPrefs.GetString(key).Should().Be(value);
        }

        [Test]
        public void GetString_ShouldReturnDefaultValueIfKeyNotFound()
        {
            // Arrange
            var key = "unknownKey";
            var defaultValue = "10";

            // Act
            var result = BinaryPrefs.GetString(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void GetString_ShouldReturnValueFromPlayerPrefsIfNotInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var playerPrefsValue = "100";

            // Store value in PlayerPrefs only
            PlayerPrefs.SetString(key, playerPrefsValue);

            // Act
            var result = BinaryPrefs.GetString(key);

            // Assert
            result.Should().Be(playerPrefsValue);

            // Verify that the value is now stored in BinaryStorage
            BinaryPrefs.GetString(key).Should().Be(playerPrefsValue);
        }

        [Test]
        public void GetString_ShouldReturnDefaultValueIfKeyNotFoundInBothStorages()
        {
            // Arrange
            var key = "nonExistentKey";
            var defaultValue = "20";

            // Act
            var result = BinaryPrefs.GetString(key, defaultValue);

            // Assert
            result.Should().Be(defaultValue);
        }

        [Test]
        public void SetString_ShouldOverrideExistingValueInBinaryStorage()
        {
            // Arrange
            var key = "key";
            var initialValue = "42";
            var newValue = "84";

            // Store initial value
            BinaryPrefs.SetString(key, initialValue);

            // Act
            BinaryPrefs.SetString(key, newValue);

            // Assert
            BinaryPrefs.GetString(key).Should().Be(newValue);
        }

        #endregion
    }
}