using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class TempFileHandlingTests : BaseStorageTests
    {
        private const int GarbageLength = 64 * 1024;

        [Test]
        public void WhenTempFileLeftFromPreviousRun_ThenSaveOverwritesItCompletely()
        {
            File.WriteAllBytes(TempPath, CreateGarbage());

            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Set("b", "two");
                storage.Save();
            }

            new FileInfo(StoragePath).Length.Should().BeLessThan(GarbageLength);
            File.Exists(TempPath).Should().BeFalse();

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            reopened.Get<int>("a").Should().Be(1);
            reopened.Get<string>("b").Should().Be("two");
        }

        [Test]
        public void WhenStorageEmptied_ThenTempAndBackupAreRemoved()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 1);
            storage.Save();
            storage.Set("b", 2);
            storage.Save();
            File.WriteAllBytes(TempPath, CreateGarbage());

            File.Exists(BackupPath).Should().BeTrue();

            storage.RemoveAll();
            storage.Save();

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        private static byte[] CreateGarbage()
        {
            var garbage = new byte[GarbageLength];
            for (var i = 0; i < garbage.Length; i++)
            {
                garbage[i] = 0xAB;
            }
            return garbage;
        }
    }
}
