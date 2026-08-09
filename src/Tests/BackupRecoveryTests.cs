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
        public void WhenStorageIsMissing_ThenBackupIsIgnored()
        {
            WriteTwoGenerations();
            File.Delete(StoragePath);

            using var storage = Open();

            storage.Has("generation").Should().BeFalse();
            File.Exists(BackupPath).Should().BeTrue();
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

        private BinaryStorage Open()
        {
            return BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
        }
    }
}
