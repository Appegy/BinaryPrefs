using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class BackupRecoveryTests : BaseStorageTests
    {
        [Test]
        public void WhenStorageIsCorrupted_ThenDataIsRecoveredFromBackup()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(1);
        }

        [Test]
        public void WhenStorageIsCorrupted_ThenBackupTakesItsPlaceOnDisk()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());

            Open().Dispose();

            File.Exists(StoragePath).Should().BeTrue();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenStorageIsCorrupted_ThenRecoveredDataSurvivesRestart()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());
            Open().Dispose();

            using var reopened = Open();

            reopened.Get<int>("generation").Should().Be(1);
        }

        [Test]
        public void WhenStorageIsTruncated_ThenDataIsRecoveredFromBackup()
        {
            WriteTwoGenerations();
            var bytes = File.ReadAllBytes(StoragePath);
            File.WriteAllBytes(StoragePath, bytes[..(bytes.Length - 8)]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(1);
        }

        [Test]
        public void WhenReplaceWasInterrupted_ThenTempIsLoadedAndTakesThePlaceOfStorage()
        {
            var generations = WriteGenerations(3);
            PutFiles(null, generations[2], generations[1]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(3);
            File.Exists(StoragePath).Should().BeTrue();
            File.Exists(TempPath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeTrue();
        }

        [Test]
        public void WhenReplaceWasInterruptedAndTempIsCut_ThenDataIsRecoveredFromBackup()
        {
            var generations = WriteGenerations(3);
            PutFiles(null, Cut(generations[2]), generations[1]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenTempIsCut_ThenStorageIsLoadedAndTempIsRemoved()
        {
            var generations = WriteGenerations(3);
            PutFiles(generations[1], Cut(generations[2]), generations[0]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSaveStoppedBeforeReplace_ThenStorageIsLoaded()
        {
            var generations = WriteGenerations(3);
            PutFiles(generations[1], generations[2], generations[0]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSaveStoppedAfterBackupWasRemoved_ThenStorageIsLoaded()
        {
            var generations = WriteGenerations(3);
            PutFiles(generations[1], generations[2], null);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSaveStoppedWhileBackupWasCopied_ThenStorageIsLoaded()
        {
            var generations = WriteGenerations(3);
            PutFiles(generations[1], generations[2], Cut(generations[1]));

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSaveStoppedAfterBackupWasCreated_ThenStorageIsLoaded()
        {
            var generations = WriteGenerations(3);
            PutFiles(generations[1], generations[2], generations[1]);

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenFirstSaveWasInterrupted_ThenStorageIsEmpty()
        {
            var generations = WriteGenerations(1);
            PutFiles(null, generations[0], null);

            using var storage = Open();

            storage.Has("generation").Should().BeFalse();
        }

        [Test]
        public void WhenOnlyBackupIsLeft_ThenStorageIsEmpty()
        {
            var generations = WriteGenerations(2);
            PutFiles(null, null, generations[0]);

            using var storage = Open();

            storage.Has("generation").Should().BeFalse();
        }

        [Test]
        public void WhenStorageIsCorruptedAndBackupIsCut_ThenThrowsAndRemovesBoth()
        {
            var generations = WriteGenerations(2);
            PutFiles(Array.Empty<byte>(), null, Cut(generations[0]));

            FluentActions.Invoking(() => Open().Dispose()).Should().Throw<StorageFileCorruptedException>();

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenStorageAndBackupAreBothCorrupted_ThenTempIsKept()
        {
            PutFiles(Array.Empty<byte>(), new byte[] { 1, 2, 3 }, Array.Empty<byte>());

            FluentActions.Invoking(() => Open().Dispose()).Should().Throw<StorageFileCorruptedException>();

            File.Exists(TempPath).Should().BeTrue();
        }

        [Test]
        public void WhenStorageAndBackupAreBothCorrupted_ThenThrowsAndRemovesBoth()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());
            File.WriteAllBytes(BackupPath, Array.Empty<byte>());

            FluentActions.Invoking(() => Open().Dispose()).Should().Throw<StorageFileCorruptedException>();

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenRestartedAfterTotalFailure_ThenStartsCleanWithoutThrowing()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());
            File.WriteAllBytes(BackupPath, Array.Empty<byte>());
            FluentActions.Invoking(() => Open().Dispose()).Should().Throw<StorageFileCorruptedException>();

            using var storage = Open();

            storage.Has("generation").Should().BeFalse();
        }

        [Test]
        public void WhenStorageIsHealthy_ThenBackupIsKeptAndOrphanedTempIsRemoved()
        {
            WriteTwoGenerations();
            File.WriteAllBytes(TempPath, new byte[] { 1, 2, 3 });

            using var storage = Open();

            storage.Get<int>("generation").Should().Be(2);
            File.Exists(BackupPath).Should().BeTrue();
            File.Exists(TempPath).Should().BeFalse();
        }

        private void WriteTwoGenerations()
        {
            using var storage = Open();
            storage.Set("generation", 1);
            storage.Save();
            storage.Set("generation", 2);
            storage.Save();
        }

        private byte[][] WriteGenerations(int count)
        {
            var generations = new byte[count][];
            using (var storage = Open())
            {
                for (var i = 0; i < count; i++)
                {
                    storage.Set("generation", i + 1);
                    storage.Save();
                    generations[i] = File.ReadAllBytes(StoragePath);
                }
            }
            return generations;
        }

        private void PutFiles(byte[] storage, byte[] temp, byte[] backup)
        {
            PutFile(StoragePath, storage);
            PutFile(TempPath, temp);
            PutFile(BackupPath, backup);
        }

        private static void PutFile(string filePath, byte[] bytes)
        {
            if (bytes == null)
            {
                File.Delete(filePath);
            }
            else
            {
                File.WriteAllBytes(filePath, bytes);
            }
        }

        private static byte[] Cut(byte[] bytes)
        {
            return bytes[..(bytes.Length - 8)];
        }
    }
}
