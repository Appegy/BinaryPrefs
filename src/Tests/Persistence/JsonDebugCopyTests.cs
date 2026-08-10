using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class JsonDebugCopyTests : BaseStorageTests
    {
        [SetUp, TearDown]
        public void CleanJsonBetweenTests()
        {
            if (File.Exists(JsonPath))
            {
                File.Delete(JsonPath);
            }
        }

        [Test]
        public void WhenDisabled_ThenNoJsonCopyWritten()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 1);
            storage.Save();

            File.Exists(StoragePath).Should().BeTrue();
            File.Exists(JsonPath).Should().BeFalse();
        }

        [Test]
        public void WhenFlagIsFalse_ThenNoJsonCopyWritten()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug(false).Build();
            storage.Set("a", 1);
            storage.Save();

            File.Exists(JsonPath).Should().BeFalse();
        }

        [Test]
        public void WhenEnabled_ThenJsonCopyWrittenNextToBinary()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug().Build();
            storage.Set("count", 7);
            storage.Set("name", "hero");
            storage.Set("ratio", 0.5f);
            storage.Set("enabled", true);
            storage.Save();

            File.Exists(JsonPath).Should().BeTrue();
            var json = File.ReadAllText(JsonPath);
            json.Should().Contain("\"count\": 7");
            json.Should().Contain("\"name\": \"hero\"");
            json.Should().Contain("\"ratio\": 0.5");
            json.Should().Contain("\"enabled\": true");
        }

        [Test]
        public void WhenStringContainsSpecialCharacters_ThenJsonIsEscaped()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug().Build();
            storage.Set("quote", "a\"b\\c\nd");
            storage.Save();

            var json = File.ReadAllText(JsonPath);
            json.Should().Contain("\"quote\": \"a\\\"b\\\\c\\nd\"");
        }

        [Test]
        public void WhenStoringVector3_ThenJsonContainsComponents()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug().Build();
            storage.Set("position", new Vector3(1f, 2f, 3f));
            storage.Save();

            var json = File.ReadAllText(JsonPath);
            json.Should().Contain("\"position\"");
            json.Should().Contain("\"x\": 1");
            json.Should().Contain("\"y\": 2");
            json.Should().Contain("\"z\": 3");
        }

        [Test]
        public void WhenStoringList_ThenJsonContainsArray()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportListsOf<int>().SaveJsonCopyForDebug().Build();
            var numbers = storage.GetListOf<int>("numbers");
            numbers.Add(10);
            numbers.Add(20);
            storage.Save();

            var json = File.ReadAllText(JsonPath);
            json.Should().Contain("\"numbers\": [");
            json.Should().Contain("10");
            json.Should().Contain("20");
        }

        [Test]
        public void WhenStoringDictionary_ThenJsonContainsObject()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportDictionariesOf<string, int>().SaveJsonCopyForDebug().Build();
            var scores = storage.GetDictionaryOf<string, int>("scores");
            scores["alice"] = 42;
            storage.Save();

            var json = File.ReadAllText(JsonPath);
            json.Should().Contain("\"scores\"");
            json.Should().Contain("\"alice\": 42");
        }

        [Test]
        public void WhenAllRemovedAndSaved_ThenJsonCopyDeleted()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug().Build();
            storage.Set("a", 1);
            storage.Save();
            File.Exists(JsonPath).Should().BeTrue();

            storage.RemoveAll();
            storage.Save();

            File.Exists(JsonPath).Should().BeFalse();
        }

        [Test]
        public void WhenToggledAtRuntime_ThenJsonCopyFollowsFlag()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 1);
            storage.Save();
            File.Exists(JsonPath).Should().BeFalse();

            storage.SaveJsonCopyForDebug = true;
            storage.Set("b", 2);
            storage.Save();

            File.Exists(JsonPath).Should().BeTrue();
        }
    }
}
