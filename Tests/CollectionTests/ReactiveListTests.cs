using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage.CollectionTests
{
    [TestFixture]
    public class ReactiveListTests
    {
        [Test]
        public void WhenItemIsAdded_AndListIsNotDisposed_ThenItemShouldBeInList()
        {
            // Arrange
            using var list = new ReactiveList<int>();

            // Act
            list.Add(1);

            // Assert
            list.Should().Contain(1);
        }

        [Test]
        public void WhenItemIsRemoved_AndItemExistsInList_ThenItemShouldNotBeInList()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            list.Remove(2);

            // Assert
            list.Should().NotContain(2);
        }

        [Test]
        public void WhenClearIsCalled_AndListHasItems_ThenListShouldBeEmpty()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            list.Clear();

            // Assert
            list.Should().BeEmpty();
        }

        [Test]
        public void WhenGettingItemByIndex_AndIndexIsValid_ThenShouldReturnCorrectItem()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            var item = list[1];

            // Assert
            item.Should().Be(2);
        }

        [Test]
        public void WhenSettingItemByIndex_AndIndexIsValid_ThenShouldUpdateItem()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            list[1] = 5;

            // Assert
            list[1].Should().Be(5);
        }

        [Test]
        public void WhenItemIsInserted_AndIndexIsValid_ThenShouldInsertItemAtCorrectPosition()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            list.Insert(1, 5);

            // Assert
            list[1].Should().Be(5);
            list[2].Should().Be(2);
        }

        [Test]
        public void WhenItemIsRemovedByIndex_AndIndexIsValid_ThenShouldRemoveItemAtCorrectPosition()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);

            // Act
            list.RemoveAt(1);

            // Assert
            list.Should().NotContain(2);
        }

        [Test]
        public void WhenListIsDisposed_ThenIsDisposedShouldBeTrue()
        {
            // Arrange
            var list = new ReactiveList<int>();

            // Act
            list.Dispose();

            // Assert
            list.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void WhenListIsDisposed_ThenListShouldBeEmpty()
        {
            // Arrange
            var list = new ReactiveList<int> { 1, 2, 3 };

            // Act
            list.Dispose();

            // Assert
            list.Should().BeEmpty();
        }

        [Test]
        public void WhenItemIsAdded_AndListIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var list = new ReactiveList<int>();
            list.Dispose();

            // Act
            Action action = () => list.Add(1);

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenGettingItemByIndex_AndListIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var list = new ReactiveList<int> { 1, 2, 3 };
            list.Dispose();

            // Act
            Action action = () => { _ = list[1]; };

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenSettingItemByIndex_AndListIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var list = new ReactiveList<int> { 1, 2, 3 };
            list.Dispose();

            // Act
            Action action = () => list[1] = 5;

            // Assert
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenItemIsAdded_AndListIsNotDisposed_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            var list = new ReactiveList<int>();
            var wasTriggered = false;
            list.OnChanged += (_) => wasTriggered = true;

            // Act
            list.Add(1);

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenItemIsRemoved_AndListIsNotDisposed_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);
            var wasTriggered = false;
            list.OnChanged += (_) => wasTriggered = true;

            // Act
            list.Remove(2);

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenListIsCleared_AndListIsNotDisposed_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            using var list = new ReactiveList<int>();
            list.AddRange(1, 2, 3);
            var wasTriggered = false;
            list.OnChanged += (_) => wasTriggered = true;

            // Act
            list.Clear();

            // Assert
            wasTriggered.Should().BeTrue();
        }
    }
}