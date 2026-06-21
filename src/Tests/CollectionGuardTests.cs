using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class CollectionGuardTests : BaseStorageTests
    {
        [Test]
        public void WhenSetCalledWithCollectionType_ThenThrows()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Invoking(s => s.Set("key", new List<int> { 1, 2, 3 })).Should().Throw<IncorrectUsageOfCollectionException>();
        }

        [Test]
        public void WhenGetCalledWithCollectionType_ThenThrows()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Invoking(s => s.Get<List<int>>("key")).Should().Throw<IncorrectUsageOfCollectionException>();
        }

        [Test]
        public void WhenSetRawCalledWithCollectionInstance_ThenThrows()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Invoking(s => s.SetRaw("key", new List<int> { 1 })).Should().Throw<IncorrectUsageOfCollectionException>();
        }

        [Test]
        public void WhenSupportsCalledWithCollectionType_ThenThrows()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Invoking(s => s.Supports<List<int>>()).Should().Throw<IncorrectUsageOfCollectionException>();
        }

        [Test]
        public void WhenStringValueUsed_ThenNotTreatedAsCollection()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("name", "John");
            storage.Get<string>("name").Should().Be("John");
        }

        [Test]
        public void WhenCollectionApiUsed_ThenWorksWithoutThrowing()
        {
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportListsOf<int>()
                .Build();

            var list = storage.GetListOf<int>("nums");
            list.Add(1);
            list.Add(2);

            storage.GetListOf<int>("nums").Should().BeEquivalentTo(new[] { 1, 2 });
        }
    }
}
