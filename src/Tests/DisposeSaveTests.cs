using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class DisposeSaveTests : BaseStorageTests
    {
        [Test]
        public void WhenDisposedInsideOpenScope_AndAutoSave_ThenChangesPersisted()
        {
            var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().EnableAutoSaveOnChange().Build();
            var scope = storage.MultipleChangeScope();
            storage.Set("a", 42);
            storage.Dispose();
            scope.Dispose();

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().EnableAutoSaveOnChange().Build();

            reopened.Get("a", 0).Should().Be(42);
        }

        [Test]
        public void WhenDisposedWithoutAutoSave_ThenChangesNotPersisted()
        {
            var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 42);
            storage.Dispose();

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            reopened.Has("a").Should().BeFalse();
        }
    }
}
