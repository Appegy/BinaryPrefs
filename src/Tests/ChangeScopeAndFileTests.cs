using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class ChangeScopeAndFileTests : BaseStorageTests
    {
        [Test]
        public void WhenNestedScopes_ThenAutoSaveDeferredUntilOutermostCloses()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().EnableAutoSaveOnChange().SaveOnBackgroundThread(false).Build();
            var outer = storage.MultipleChangeScope();
            var inner = storage.MultipleChangeScope();

            storage.Set("a", 1);
            inner.Dispose();
            File.Exists(StoragePath).Should().BeFalse();

            outer.Dispose();
            File.Exists(StoragePath).Should().BeTrue();
        }

        [Test]
        public void WhenAllRemovedAndSaved_ThenFileDeleted()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 1);
            storage.Save();
            File.Exists(StoragePath).Should().BeTrue();

            storage.RemoveAll();
            storage.Save();

            File.Exists(StoragePath).Should().BeFalse();
        }
    }
}
