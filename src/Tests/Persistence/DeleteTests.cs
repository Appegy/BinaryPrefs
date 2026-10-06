using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class DeleteTests : BaseStorageTests
    {
        [Test]
        public void WhenStorageDeleted_ThenEveryFileIsGone()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SaveJsonCopyForDebug(true).Build())
            {
                storage.Set("a", 1);
                storage.Save();
                storage.Set("a", 2);
                storage.Save();
            }
            File.WriteAllBytes(TempPath, new byte[] { 1, 2, 3 });

            BinaryStorage.Delete(StoragePath);

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
            File.Exists(JsonPath).Should().BeFalse();
        }

        [Test]
        public void WhenOnlyTempAndBackupExist_ThenBothAreDeleted()
        {
            File.WriteAllBytes(TempPath, new byte[] { 1, 2, 3 });
            File.WriteAllBytes(BackupPath, new byte[] { 4, 5, 6 });

            BinaryStorage.Delete(StoragePath);

            File.Exists(TempPath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenStorageIsOpen_ThenDeleteThrowsInEditor()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Set("a", 1);
            storage.Save();

            FluentActions.Invoking(() => BinaryStorage.Delete(StoragePath)).Should().Throw<Exception>();

            File.Exists(StoragePath).Should().BeTrue();
        }

        [Test]
        public void WhenNoFilesExist_ThenDeleteDoesNothing()
        {
            FluentActions.Invoking(() => BinaryStorage.Delete(StoragePath)).Should().NotThrow();

            File.Exists(StoragePath).Should().BeFalse();
        }
    }
}
