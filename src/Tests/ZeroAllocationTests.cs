using System;
using System.Collections.Generic;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    [Category("Allocations")]
    public class ZeroAllocationTests : BaseStorageTests
    {
        public enum Level
        {
            One,
            Two,
            Three
        }

        private int _intSink;
        private string _stringSink;
        private object _objectSink;
        private bool _boolSink;
        private Type _typeSink;
        private Vector3 _vectorSink;

        private BinaryStorage CreateStorage()
        {
            var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportEnum<Level>()
                .SupportListsOf<int>()
                .SupportSetsOf<int>()
                .SupportDictionariesOf<int, int>()
                .Build();

            storage.Set("int", 42);
            storage.Set("string", "hello");
            storage.Set("vector", new Vector3(1, 2, 3));
            storage.Set("enum", Level.Two);
            return storage;
        }

        private static void ShouldNotAllocate(string api, Action action)
        {
            var bytesPerCall = AllocationProbe.BytesPerCall(action);
            bytesPerCall.Should().BeLessThan(AllocationProbe.AllowedBytesPerCall, "{0} must not allocate, but measured {1:F1} bytes per call", api, bytesPerCall);
        }

        #region Reads

        [Test]
        public void WhenGetIntCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Get<int>", () => _intSink += storage.Get<int>("int"));
        }

        [Test]
        public void WhenGetStringCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Get<string>", () => _stringSink = storage.Get<string>("string"));
        }

        [Test]
        public void WhenGetVectorCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Get<Vector3>", () => _vectorSink = storage.Get<Vector3>("vector"));
        }

        [Test]
        public void WhenGetEnumCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Get<Level>", () => _intSink += (int)storage.Get<Level>("enum"));
        }

        [Test]
        public void WhenGetMissingKeyCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Get<int> (missing key)", () => _intSink += storage.Get("missing", 7, MissingKeyBehavior.ReturnDefaultValueOnly));
        }

        [Test]
        public void WhenGetRawOfReferenceTypeCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("GetRaw (reference type)", () => _objectSink = storage.GetRaw("string"));
        }

        [Test]
        public void WhenHasCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Has", () => _boolSink ^= storage.Has("int"));
        }

        [Test]
        public void WhenTypeOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("TypeOf", () => _typeSink = storage.TypeOf("int"));
        }

        [Test]
        public void WhenKeysCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Keys", () => _objectSink = storage.Keys);
        }

        #endregion

        #region Writes

        [Test]
        public void WhenSetCalledWithSameValue_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Set<int> (unchanged)", () => _boolSink ^= storage.Set("int", 42));
        }

        [Test]
        public void WhenSetCalledWithNewValue_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var toggle = 0;
            ShouldNotAllocate("Set<int> (changed)", () =>
            {
                toggle ^= 1;
                _boolSink ^= storage.Set("int", toggle);
            });
        }

        [Test]
        public void WhenRemoveOfMissingKeyCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Remove (missing key)", () => _boolSink ^= storage.Remove("missing"));
        }

        [Test]
        public void WhenRemoveByPredicateMatchesNothing_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Remove (predicate)", () => _intSink += storage.Remove(static _ => false));
        }

        #endregion

        #region Capability queries

        [Test]
        public void WhenSupportsCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("Supports<int>", () => _boolSink ^= storage.Supports<int>());
        }

        [Test]
        public void WhenSupportsListsOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("SupportsListsOf<int>", () => _boolSink ^= storage.SupportsListsOf<int>());
        }

        [Test]
        public void WhenSupportsSetsOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("SupportsSetsOf<int>", () => _boolSink ^= storage.SupportsSetsOf<int>());
        }

        [Test]
        public void WhenSupportsDictionariesOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("SupportsDictionariesOf<int, int>", () => _boolSink ^= storage.SupportsDictionariesOf<int, int>());
        }

        #endregion

        #region Collections

        [Test]
        public void WhenGetListOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            storage.GetListOf<int>("list").Add(1);
            ShouldNotAllocate("GetListOf<int>", () => _objectSink = storage.GetListOf<int>("list"));
        }

        [Test]
        public void WhenGetSetOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            storage.GetSetOf<int>("set").Add(1);
            ShouldNotAllocate("GetSetOf<int>", () => _objectSink = storage.GetSetOf<int>("set"));
        }

        [Test]
        public void WhenGetDictionaryOfCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            storage.GetDictionaryOf<int, int>("dictionary").Add(1, 1);
            ShouldNotAllocate("GetDictionaryOf<int, int>", () => _objectSink = storage.GetDictionaryOf<int, int>("dictionary"));
        }

        [Test]
        public void WhenListIndexerRead_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var list = storage.GetListOf<int>("list");
            list.Add(1);
            ShouldNotAllocate("IList<int> indexer", () => _intSink += list[0]);
        }

        [Test]
        public void WhenListMutated_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var list = storage.GetListOf<int>("list");
            list.Add(1);
            ShouldNotAllocate("IList<int> add and remove", () =>
            {
                list.Add(2);
                list.RemoveAt(list.Count - 1);
            });
        }

        [Test]
        [Ignore("Enumerating through IList<T> boxes the struct enumerator. Fixing it means returning concrete collection types, which changes the public API.")]
        public void WhenListEnumerated_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var list = storage.GetListOf<int>("list");
            for (var i = 0; i < 8; i++)
            {
                list.Add(i);
            }
            ShouldNotAllocate("foreach over IList<int>", () =>
            {
                foreach (var value in list)
                {
                    _intSink += value;
                }
            });
        }

        [Test]
        [Ignore("Enumerating through ISet<T> boxes the struct enumerator. Fixing it means returning concrete collection types, which changes the public API.")]
        public void WhenSetEnumerated_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var set = storage.GetSetOf<int>("set");
            for (var i = 0; i < 8; i++)
            {
                set.Add(i);
            }
            ShouldNotAllocate("foreach over ISet<int>", () =>
            {
                foreach (var value in set)
                {
                    _intSink += value;
                }
            });
        }

        [Test]
        [Ignore("Enumerating through IDictionary<TKey, TValue> boxes the struct enumerator. Fixing it means returning concrete collection types, which changes the public API.")]
        public void WhenDictionaryEnumerated_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var dictionary = storage.GetDictionaryOf<int, int>("dictionary");
            for (var i = 0; i < 8; i++)
            {
                dictionary.Add(i, i);
            }
            ShouldNotAllocate("foreach over IDictionary<int, int>", () =>
            {
                foreach (var pair in dictionary)
                {
                    _intSink += pair.Value;
                }
            });
        }

        #endregion

        #region Change scope

        [Test]
        [Ignore("Returning IDisposable forces an allocation. Fixing it means returning a struct scope, which changes the public API.")]
        public void WhenMultipleChangeScopeUsed_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            ShouldNotAllocate("MultipleChangeScope", () =>
            {
                using (storage.MultipleChangeScope())
                {
                }
            });
        }

        #endregion

        #region Serialization

        [Test]
        public void WhenDataSerializedIntoBuffer_ThenNothingAllocated()
        {
            var sections = new List<BinarySection>
            {
                new TypedBinarySection<int>(Int32Serializer.Shared),
                new TypedBinarySection<string>(StringSerializer.Shared)
            };
            var data = new Dictionary<string, Record>
            {
                { "int", new Record<int>(42, 0) },
                { "string", new Record<string>("hello", 1) }
            };
            sections[0].Count++;
            sections[1].Count++;

            ShouldNotAllocate("BinaryStorageIO.SerializeToBuffer", () =>
            {
                var stream = BinaryStorageIO.SerializeToBuffer(sections, data);
                _intSink += (int)stream.Length;
                stream.Release();
            });
        }

        #endregion

        #region Nested storage

        [Test]
        public void WhenNestedGetCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var nested = storage.CreateChild("child");
            nested.Set("int", 42);
            ShouldNotAllocate("NestedBinaryStorage.Get<int>", () => _intSink += nested.Get<int>("int"));
        }

        [Test]
        public void WhenNestedHasCalled_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var nested = storage.CreateChild("child");
            nested.Set("int", 42);
            ShouldNotAllocate("NestedBinaryStorage.Has", () => _boolSink ^= nested.Has("int"));
        }

        [Test]
        public void WhenNestedSetCalledWithSameValue_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var nested = storage.CreateChild("child");
            nested.Set("int", 42);
            ShouldNotAllocate("NestedBinaryStorage.Set<int>", () => _boolSink ^= nested.Set("int", 42));
        }

        [Test]
        public void WhenNestedGetCalledAfterKeyCacheCleanup_ThenNothingAllocated()
        {
            using var storage = CreateStorage();
            var nested = storage.CreateChild("child");
            nested.Set("int", 42);
            for (var i = 0; i < 1000; i++)
            {
                nested.Has($"missing{i}");
            }
            ShouldNotAllocate("NestedBinaryStorage.Get<int> after key cache cleanup", () => _intSink += nested.Get<int>("int"));
        }

        #endregion
    }
}
