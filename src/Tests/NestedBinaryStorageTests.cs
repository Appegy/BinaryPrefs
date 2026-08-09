using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class NestedBinaryStorageTests : BaseStorageTests
    {
        [Test]
        public void WhenKeyAddedToNested_ThenRootHasPrefixedKey()
        {
            // Verify key formation: __prefix->key
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .Build();

            var nested = root.CreateChild("level1");

            nested.Set("score", 100);

            // Nested storage should see its own key "score"
            nested.Get<int>("score").Should().Be(100);

            // Root should see ONLY the prefixed key
            root.Has("__level1->score").Should().BeTrue();
            root.Get<int>("__level1->score").Should().Be(100);

            // Root should NOT see the raw key
            root.Has("score").Should().BeFalse();
        }

        [Test]
        public void WhenSameKeyUsedInRootAndNested_ThenTheyAreIndependent()
        {
            // Verify that keys do not overlap
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .Build();

            var nested = root.CreateChild("settings");

            root.Set("volume", 1.0f);
            nested.Set("volume", 0.5f);

            // They should hold different values
            root.Get<float>("volume").Should().Be(1.0f);
            nested.Get<float>("volume").Should().Be(0.5f);
        }

        [Test]
        public void WhenCreatingNestedInsideNested_ThenPrefixesStackRecursively()
        {
            // Verify recursion: __p1->__p2->key
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .Build();

            var userStorage = root.CreateChild("user_1");
            var inventoryStorage = userStorage.CreateChild("inventory");

            inventoryStorage.Set("gold", 500);

            inventoryStorage.Get<int>("gold").Should().Be(500);

            // Check the full path in root
            root.Has("__user_1->__inventory->gold").Should().BeTrue();
        }

        [Test]
        public void WhenRemoveAllCalledOnNested_ThenOnlyNestedKeysAreRemoved()
        {
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .Build();

            var nested = root.CreateChild("session");

            root.Set("root_item", 1); // Should NOT be removed
            nested.Set("item1", 10); // Should be removed
            nested.Set("item2", 20); // Should be removed

            var removedCount = nested.RemoveAll();

            removedCount.Should().Be(2);
            nested.Has("item1").Should().BeFalse();
            root.Has("root_item").Should().BeTrue();
        }

        [Test]
        public void WhenRemoveByPredicateCalledOnNested_ThenPredicateReceivesSimpleKeys()
        {
            // Crucial test: predicate should receive "key", not "__prefix->key"
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .Build();

            var nested = root.CreateChild("filter");

            nested.Set("keep_me", 1);
            nested.Set("del_me", 2);

            // Remove everything starting with "del" (inside the nested scope)
            var removed = nested.Remove(key => key.StartsWith("del"));

            removed.Should().Be(1);
            nested.Has("keep_me").Should().BeTrue();
            nested.Has("del_me").Should().BeFalse();
        }

        [Test]
        public void WhenUsingNestedList_ThenItIsStoredWithPrefix()
        {
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportListsOf<int>()
                .Build();

            var nested = root.CreateChild("data");

            var list = nested.GetListOf<int>("my_list");
            list.Add(5);
            list.Add(10);

            // Check via nested
            nested.GetListOf<int>("my_list").Should().ContainInOrder(5, 10);

            // Check via root (key must have prefix)
            root.Has("__data->my_list").Should().BeTrue();

            // Ensure it is the same list instance or holds same data
            root.GetListOf<int>("__data->my_list").Should().ContainInOrder(5, 10);
        }

        [Test]
        public void WhenUsingNestedSet_ThenItIsStoredWithPrefix()
        {
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportSetsOf<string>()
                .Build();

            var nested = root.CreateChild("tags_scope");

            var set = nested.GetSetOf<string>("tags");
            set.Add("hero");
            set.Add("npc");

            // Check
            nested.GetSetOf<string>("tags").Should().Contain("hero");
            root.Has("__tags_scope->tags").Should().BeTrue();
        }

        [Test]
        public void WhenUsingNestedDictionary_ThenItIsStoredWithPrefix()
        {
            using var root = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportDictionariesOf<string, int>()
                .Build();

            var nested = root.CreateChild("stats_scope");

            var dict = nested.GetDictionaryOf<string, int>("stats");
            dict.Add("str", 10);
            dict.Add("dex", 15);

            // Check
            nested.GetDictionaryOf<string, int>("stats")["str"].Should().Be(10);
            root.Has("__stats_scope->stats").Should().BeTrue();

            // Check ReadOnly access
            nested.GetReadOnlyDictionaryOf<string, int>("stats")["dex"].Should().Be(15);
        }
    }
}
