using System;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage.CollectionTests
{
    [TestFixture]
    public class ReactiveSetTests
    {
        [Test]
        public void WhenItemIsAdded_AndSetIsNotDisposed_ThenItemShouldBeInSet()
        {
            // Arrange
            var set = new ReactiveSet<int>();

            // Act
            set.Add(1);

            // Assert
            set.Should().Contain(1);
        }

        [Test]
        public void WhenItemIsRemoved_AndItemExistsInSet_ThenItemShouldNotBeInSet()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.Remove(2);

            // Assert
            set.Should().NotContain(2);
        }

        [Test]
        public void WhenClearIsCalled_AndSetHasItems_ThenSetShouldBeEmpty()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.Clear();

            // Assert
            set.Should().BeEmpty();
        }

        [Test]
        public void WhenItemIsAdded_AndSetIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var set = new ReactiveSet<int>();
            set.Dispose();

            // Act
            Action action = () => set.Add(1);

            // Assert
            action.Should().Throw<CollectionDisposedException>();
        }

        [Test]
        public void WhenItemIsRemoved_AndSetIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };
            set.Dispose();

            // Act
            Action action = () => set.Remove(1);

            // Assert
            action.Should().Throw<CollectionDisposedException>();
        }

        [Test]
        public void WhenClearIsCalled_AndSetIsDisposed_ThenShouldThrowException()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };
            set.Dispose();

            // Act
            Action action = () => set.Clear();

            // Assert
            action.Should().Throw<CollectionDisposedException>();
        }

        [Test]
        public void WhenExceptWithIsCalled_AndSetIsNotDisposed_ThenShouldRemoveItems()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.ExceptWith(new[] { 2, 3, 4 });

            // Assert
            set.Should().Contain(1);
            set.Should().NotContain(new[] { 2, 3 });
        }

        [Test]
        public void WhenIntersectWithIsCalled_AndSetIsNotDisposed_ThenShouldRetainOnlyCommonItems()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.IntersectWith(new[] { 2, 3, 4 });

            // Assert
            set.Should().Contain(new[] { 2, 3 });
            set.Should().NotContain(new[] { 1, 4 });
        }

        [Test]
        public void WhenSymmetricExceptWithIsCalled_AndSetIsNotDisposed_ThenShouldRetainUniqueItems()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.SymmetricExceptWith(new[] { 2, 3, 4 });

            // Assert
            set.Should().Contain(new[] { 1, 4 });
            set.Should().NotContain(new[] { 2, 3 });
        }

        [Test]
        public void WhenUnionWithIsCalled_AndSetIsNotDisposed_ThenShouldIncludeAllUniqueItems()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.UnionWith(new[] { 2, 3, 4 });

            // Assert
            set.Should().Contain(new[] { 1, 2, 3, 4 });
        }

        [Test]
        public void WhenSetIsDisposed_ThenIsDisposedShouldBeTrue()
        {
            // Arrange
            var set = new ReactiveSet<int>();

            // Act
            set.Dispose();

            // Assert
            set.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void WhenSetIsDisposed_ThenSetShouldBeEmpty()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };

            // Act
            set.Dispose();

            // Assert
            set.Should().BeEmpty();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndSetIsModified_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            var set = new ReactiveSet<int>();
            var wasTriggered = false;
            set.OnChanged += () => wasTriggered = true;

            // Act
            set.Add(1);

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndSetIsCleared_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };
            var wasTriggered = false;
            set.OnChanged += () => wasTriggered = true;

            // Act
            set.Clear();

            // Assert
            wasTriggered.Should().BeTrue();
        }

        [Test]
        public void WhenOnChangedIsSubscribed_AndSetIsDisposed_ThenOnChangedShouldBeTriggered()
        {
            // Arrange
            var set = new ReactiveSet<int> { 1, 2, 3 };
            var wasTriggered = false;
            set.OnChanged += () => wasTriggered = true;

            // Act
            set.Dispose();

            // Assert
            wasTriggered.Should().BeTrue();
        }
    }
}
