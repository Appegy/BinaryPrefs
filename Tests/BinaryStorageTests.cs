using System;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class BinaryStorageTests : BaseStorageTests
    {
        [Test]
        public void WhenStorageCreated_AndPrimitiveTypesAdded_ThenAllStandardTypesSupported()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
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
            using var storage = BinaryStorage.Construct(StoragePath)
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
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            using (storage.MultipleChangeScope())
            {
                storage.Set("prefix1.key1", 11);
                storage.Set("prefix1.key2", 12);
                storage.Set("prefix2.key1", 21);
                storage.Set("prefix2.key2", 22);
            }

            // Act
            storage.Remove(key => key.StartsWith("prefix1"));

            // Assert
            storage.Has("prefix1.key1").Should().Be(false);
            storage.Has("prefix1.key2").Should().Be(false);
            storage.Has("prefix2.key1").Should().Be(true);
            storage.Has("prefix2.key2").Should().Be(true);
        }

        [Test]
        public void WhenStorageDisposed_AndHasCalled_ThenExceptionOccured()
        {
            // Arrange
            var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            // Act
            storage.Dispose();

            // Assert
            Action action = () => storage.Has("key");
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenStorageDisposed_AndTypeOfCalled_ThenExceptionOccured()
        {
            // Arrange
            var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            // Act
            storage.Dispose();

            // Assert
            Action action = () => storage.TypeOf("key");
            action.Should().Throw<ObjectDisposedException>();
        }

        [Test]
        public void WhenStorageDisposed_AndSupportsCalled_ThenExceptionOccured()
        {
            // Arrange
            var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            // Act
            storage.Dispose();

            // Assert
            Action action = () => storage.Supports<int>();
            action.Should().Throw<ObjectDisposedException>();
        }

        #region Reactive Lists

        [Test]
        public void WhenReactiveListAddedDuringBuilding_ThenStorageSupportsIt()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            // Assert
            storage.SupportsListsOf<int>().Should().Be(true);
        }

        [Test]
        public void WhenReactiveListChanged_ThenValuesInStorageCorrect()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            // Act
            var list = storage.GetListOf<int>("numbers");
            list.Add(1);
            list.Add(2);

            // Assert
            storage.GetListOf<int>("numbers").Should().BeSameAs(list);
            storage.GetListOf<int>("numbers").Should().Equal(list);
        }

        [Test]
        public void WhenReactiveListRemoved_AndNewRecordCreated_ThenNoException()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            // Act
            storage.GetListOf<int>("numbers");
            storage.Remove("numbers");

            // Assert
            storage.Has("numbers").Should().Be(false);
        }

        [Test]
        public void WhenReactiveListChanged_AndStorageReloaded_ThenValuesInStorageCorrect()
        {
            // Arrange
            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .SupportListsOf<int>()
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                // Act
                var list = storage.GetListOf<int>("numbers");
                list.Add(1);
                list.Add(2);
            }

            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .SupportListsOf<int>()
                       .Build())
            {
                // Assert
                storage.Has("numbers");
                var list = storage.GetListOf<int>("numbers");
                list.Count.Should().Be(2);
                list[0].Should().Be(1);
                list[1].Should().Be(2);
            }
        }

        #endregion

        #region Reactive Sets

        [Test]
        public void WhenReactiveSetAddedDuringBuilding_ThenStorageSupportsIt()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportSetsOf<int>()
                .Build();

            // Assert
            storage.SupportsSetsOf<int>().Should().Be(true);
        }

        [Test]
        public void WhenReactiveSetChanged_ThenValuesInStorageCorrect()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportSetsOf<int>()
                .Build();

            // Act
            var set = storage.GetSetOf<int>("numbers");
            set.Add(1);
            set.Add(2);

            // Assert
            storage.GetSetOf<int>("numbers").Should().BeSameAs(set);
            storage.GetSetOf<int>("numbers").Should().Equal(set);
        }

        [Test]
        public void WhenReactiveSetRemoved_AndNewRecordCreated_ThenNoException()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportSetsOf<int>()
                .Build();

            // Act
            storage.GetSetOf<int>("numbers");
            storage.Remove("numbers");

            // Assert
            storage.Has("numbers").Should().Be(false);
        }

        [Test]
        public void WhenReactiveSetChanged_AndStorageReloaded_ThenValuesInStorageCorrect()
        {
            // Arrange
            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .SupportSetsOf<int>()
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                // Act
                var set = storage.GetSetOf<int>("numbers");
                set.Add(1);
                set.Add(2);
            }

            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .SupportSetsOf<int>()
                       .Build())
            {
                // Assert
                storage.Has("numbers");
                var set = storage.GetSetOf<int>("numbers");
                set.Count.Should().Be(2);
                set.Should().Contain(1);
                set.Should().Contain(2);
            }
        }

        #endregion

        #region Reactive Dictionaries

        [Test]
        public void WhenReactiveDictionaryAddedDuringBuilding_ThenStorageSupportsIt()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .AddTypeSerializer(StringSerializer.Shared)
                .SupportDictionariesOf<int, string>()
                .Build();

            // Assert
            storage.SupportsDictionariesOf<int, string>().Should().Be(true);
        }

        [Test]
        public void WhenReactiveDictionaryChanged_ThenValuesInStorageCorrect()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .AddTypeSerializer(StringSerializer.Shared)
                .SupportDictionariesOf<int, string>()
                .Build();

            // Act
            var map = storage.GetDictionaryOf<int, string>("numbers");
            map[1] = "one";
            map.Add(2, "two");

            // Assert
            storage.GetDictionaryOf<int, string>("numbers").Should().BeSameAs(map);
            storage.GetDictionaryOf<int, string>("numbers").Should().Equal(map);
        }

        [Test]
        public void WhenReactiveDictionaryRemoved_AndNewRecordCreated_ThenNoException()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .AddTypeSerializer(StringSerializer.Shared)
                .SupportDictionariesOf<int, string>()
                .Build();

            // Act
            storage.GetDictionaryOf<int, string>("numbers");
            storage.Remove("numbers");

            // Assert
            storage.Has("numbers").Should().Be(false);
        }

        [Test]
        public void WhenReactiveDictionaryChanged_AndStorageReloaded_ThenValuesInStorageCorrect()
        {
            // Arrange
            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .SupportDictionariesOf<int, string>()
                       .EnableAutoSaveOnChange()
                       .Build())
            {
                // Act
                var map = storage.GetDictionaryOf<int, string>("numbers");
                map[1] = "one";
                map.Add(2, "two");
            }

            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddTypeSerializer(Int32Serializer.Shared)
                       .AddTypeSerializer(StringSerializer.Shared)
                       .SupportDictionariesOf<int, string>()
                       .Build())
            {
                // Assert
                storage.Has("numbers");
                var map = storage.GetDictionaryOf<int, string>("numbers");
                map.Count.Should().Be(2);
                map.Should().ContainKeys(1, 2);
                map[1].Should().Be("one");
                map[2].Should().Be("two");
            }
        }

        #endregion

        #region Events

        [Test]
        public void WhenKeyAddedToStorage_ThenOnKeyAddedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            var raised = false;
            storage.OnKeyAdded += s => { raised = s == "key"; };

            // Act
            storage.Set("key", 10);

            // Assert
            raised.Should().BeTrue("OnKeyAdded should be raised when Set is called.");
        }

        [Test]
        public void WhenKeyRemovedFromStorage_ThenOnKeyRemovedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            storage.Set("key", 10);

            var raised = false;
            storage.OnKeyRemoved += s => { raised = s == "key"; };

            // Act
            storage.Remove("key");

            // Assert
            raised.Should().BeTrue("OnKeyRemoved should be raised when Remove is called.");
        }

        [Test]
        public void WhenKeyChangedInStorage_ThenOnKeyChangedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .Build();

            storage.Set("key", 10);

            var raised = false;
            storage.OnKeyChanged += s => { raised = s == "key"; };

            // Act
            storage.Set("key", 20);

            // Assert
            raised.Should().BeTrue("OnKeyChanged should be raised when Set is called.");
        }

        [Test]
        public void WhenCollectionAddedToStorage_ThenOnKeyAddedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            var raised = false;
            storage.OnKeyAdded += s => { raised = s == "key"; };

            // Act
            storage.GetListOf<int>("key").Add(10);

            // Assert
            raised.Should().BeTrue("OnKeyAdded should be raised when Set is called.");
        }

        [Test]
        public void WhenCollectionRemovedFromStorage_ThenOnKeyRemovedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            storage.GetListOf<int>("key").Add(10);

            var raised = false;
            storage.OnKeyRemoved += s => { raised = s == "key"; };

            // Act
            storage.Remove("key");

            // Assert
            raised.Should().BeTrue("OnKeyRemoved should be raised when Remove is called.");
        }

        [Test]
        public void WhenCollectionChangedInStorage_ThenOnKeyChangedEventRaised()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddTypeSerializer(Int32Serializer.Shared)
                .SupportListsOf<int>()
                .Build();

            storage.GetListOf<int>("key").Add(10);

            var raised = false;
            storage.OnKeyChanged += s => { raised = s == "key"; };

            // Act
            storage.GetListOf<int>("key").Add(20);

            // Assert
            raised.Should().BeTrue("OnKeyChanged should be raised when Set is called.");
        }

        #endregion

        #region TypeMismatchBehaviour Tests

        [Test]
        public void WhenTypeMismatchBehaviorIsThrowException_ThenExceptionIsThrown()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetTypeMismatchBehaviour(TypeMismatchBehaviour.ThrowException)
                .Build();

            storage.Set("key", 123);

            // Act
            // ReSharper disable once AccessToDisposedClosure
            Action action = () => storage.Set("key", "value");

            // Assert
            action.Should().Throw<UnexpectedTypeException>();
        }

        [Test]
        public void WhenTypeMismatchBehaviorIsOverrideValueAndType_ThenValueAndTypeAreOverridden()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetTypeMismatchBehaviour(TypeMismatchBehaviour.OverrideValueAndType)
                .Build();

            storage.Set("key", 123);

            // Act
            var result = storage.Set("key", "value");

            // Assert
            result.Should().BeTrue();
            storage.TypeOf("key").Should().Be(typeof(string));
            storage.Get<string>("key").Should().Be("value");
        }

        [Test]
        public void WhenTypeMismatchBehaviorIsIgnore_ThenValueAndTypeAreIgnored()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetTypeMismatchBehaviour(TypeMismatchBehaviour.Ignore)
                .Build();

            storage.Set("key", 123);

            // Act
            var result = storage.Set("key", "value");

            // Assert
            result.Should().BeFalse();
            storage.TypeOf("key").Should().Be(typeof(int));
            storage.Get<int>("key").Should().Be(123);
        }

        [Test]
        public void WhenTypeMismatchBehaviorOverride_ThenBehaviorIsOverridden()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetTypeMismatchBehaviour(TypeMismatchBehaviour.ThrowException)
                .Build();

            storage.Set("key", 123);

            // Act
            var result = storage.Set("key", "value", TypeMismatchBehaviour.OverrideValueAndType);

            // Assert
            result.Should().BeTrue();
            storage.Has("key").Should().BeTrue();
            storage.TypeOf("key").Should().Be(typeof(string));
            storage.Get<string>("key").Should().Be("value");
        }

        #endregion

        #region MissingKeyBehavior Tests

        [Test]
        public void WhenMissingKeyBehaviorIsInitializeWithDefaultValue_ThenKeyIsInitialized()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetMissingKeyBehaviour(MissingKeyBehavior.InitializeWithDefaultValue)
                .Build();

            // Act
            var value = storage.Get("key", 10);

            // Assert
            value.Should().Be(10);
            storage.Has("key").Should().BeTrue();
            storage.Get<int>("key").Should().Be(10);
        }

        [Test]
        public void WhenMissingKeyBehaviorIsReturnDefaultValueOnly_ThenDefaultValueIsReturned()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetMissingKeyBehaviour(MissingKeyBehavior.ReturnDefaultValueOnly)
                .Build();

            // Act
            var value = storage.Get("key", 10);

            // Assert
            value.Should().Be(10);
            storage.Has("key").Should().BeFalse();
        }

        [Test]
        public void WhenMissingKeyBehaviorOverrideIsSetInGetMethod_ThenBehaviorIsOverridden()
        {
            // Arrange
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SetMissingKeyBehaviour(MissingKeyBehavior.ReturnDefaultValueOnly)
                .Build();

            // Act
            var value = storage.Get("key", 10, MissingKeyBehavior.InitializeWithDefaultValue);

            // Assert
            value.Should().Be(10);
            storage.Has("key").Should().BeTrue();
            storage.Get<int>("key").Should().Be(10);
        }

        #endregion
    }
}