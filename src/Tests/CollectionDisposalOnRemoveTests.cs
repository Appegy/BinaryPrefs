using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class CollectionDisposalOnRemoveTests : BaseStorageTests
    {
        [Test]
        public void WhenKeyWithCollectionRemoved_ThenCollectionDisposed()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportListsOf<int>().Build();
            var list = (ReactiveList<int>)storage.GetListOf<int>("nums");

            storage.Remove("nums");

            list.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void WhenRemoveAll_ThenCollectionsDisposed()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportListsOf<int>().Build();
            var list = (ReactiveList<int>)storage.GetListOf<int>("nums");

            storage.RemoveAll();

            list.IsDisposed.Should().BeTrue();
        }
    }
}
