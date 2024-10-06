using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage.CollectionTests
{
    [TestFixture]
    public class ReactiveDictionaryTests
    {
        [Test]
        public void WhenItemIsAdded_AndDictionaryIsNotDisposed_ThenItemShouldBeInDictionary()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();

            // Act
            dictionary.Add(1, "one");

            // Assert
            dictionary.Should().Contain(new KeyValuePair<int, string>(1, "one"));
        }

        [Test]
        public void WhenItemIsRemoved_AndItemExistsInDictionary_ThenItemShouldNotBeInDictionary()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));

            // Act
            dictionary.Remove(2);

            // Assert
            dictionary.Should().NotContain(new KeyValuePair<int, string>(2, "two"));
        }

        [Test]
        public void WhenClearIsCalled_AndDictionaryHasItems_ThenDictionaryShouldBeEmpty()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));

            // Act
            dictionary.Clear();

            // Assert
            dictionary.Should().BeEmpty();
        }

        [Test]
        public void WhenGettingItemByKey_AndKeyIsValid_ThenShouldReturnCorrectItem()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));

            // Act
            var item = dictionary[1];

            // Assert
            item.Should().Be("one");
        }

        [Test]
        public void WhenSettingItemByKey_AndKeyIsValid_ThenShouldUpdateItem()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));

            // Act
            dictionary[1] = "uno";

            // Assert
            dictionary[1].Should().Be("uno");
        }

        [Test]
        public void WhenItemIsAdded_AndDictionaryIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string>();
            dictionary.Dispose();

            // Act
            Action action = () => dictionary.Add(1, "one");

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenItemIsRemoved_AndDictionaryIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string> { { 1, "one" }, { 2, "two" } };
            dictionary.Dispose();

            // Act
            Action action = () => dictionary.Remove(1);

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenClearIsCalled_AndDictionaryIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string> { { 1, "one" }, { 2, "two" } };
            dictionary.Dispose();

            // Act
            Action action = () => dictionary.Clear();

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenSetItemByKey_AndDictionaryIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string> { { 1, "one" }, { 2, "two" } };
            dictionary.Dispose();

            // Act
            Action action = () => dictionary[1] = "uno";

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenDictionaryIsDisposed_ThenIsDisposedShouldBeTrue()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string>();

            // Act
            dictionary.Dispose();

            // Assert
            dictionary.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void WhenDictionaryIsDisposed_ThenDictionaryShouldBeEmpty()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string> { { 1, "one" }, { 2, "two" } };

            // Act
            dictionary.Dispose();

            // Assert
            dictionary.Should().BeEmpty();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndDictionaryIsModified_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            var wasTriggered = false;
            dictionary.OnChanged += (_) => wasTriggered = true;

            // Act
            dictionary.Add(1, "one");

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndDictionaryIsCleared_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));
            var wasTriggered = false;
            dictionary.OnChanged += (_) => wasTriggered = true;

            // Act
            dictionary.Clear();

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndDictionaryIsDisposed_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            var dictionary = new ReactiveDictionary<int, string> { { 1, "one" }, { 2, "two" } };
            var wasTriggered = false;
            dictionary.OnChanged += (_) => wasTriggered = true;

            // Act
            dictionary.Dispose();

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenKeyExists_AndTryGetValueIsCalled_ThenShouldReturnTrueAndCorrectValue()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.Add(1, "one");

            // Act
            var result = dictionary.TryGetValue(1, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be("one");
        }

        [Test]
        public void WhenKeyDoesNotExist_AndTryGetValueIsCalled_ThenShouldReturnFalse()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();

            // Act
            var result = dictionary.TryGetValue(1, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Test]
        public void WhenContainsKeyIsCalled_AndKeyExists_ThenShouldReturnTrue()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.Add(1, "one");

            // Act
            var result = dictionary.ContainsKey(1);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void WhenContainsKeyIsCalled_AndKeyDoesNotExist_ThenShouldReturnFalse()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();

            // Act
            var result = dictionary.ContainsKey(1);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void WhenCopyToIsCalled_ThenDictionaryShouldBeCopiedToArray()
        {
            // Arrange
            using var dictionary = new ReactiveDictionary<int, string>();
            dictionary.AddRange((1, "one"), (2, "two"));
            var array = new KeyValuePair<int, string>[2];

            // Act
            dictionary.CopyTo(array, 0);

            // Assert
            array.Should().Contain(new KeyValuePair<int, string>(1, "one"));
            array.Should().Contain(new KeyValuePair<int, string>(2, "two"));
        }
    }
}