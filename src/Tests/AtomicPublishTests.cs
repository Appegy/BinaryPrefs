using System.Collections.Generic;
using System.IO;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class AtomicPublishTests : BaseStorageTests
    {
        [Test]
        public void WhenSavedFirstTime_ThenNoBackupCreated()
        {
            var (sections, data) = CreateSample(1);

            BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data);

            File.Exists(StoragePath).Should().BeTrue();
            File.Exists(BackupPath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSavedOverExistingFile_ThenBackupHoldsPreviousGeneration()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);
            var firstGeneration = File.ReadAllBytes(StoragePath);

            var (secondSections, secondData) = CreateSample(2);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, secondSections, secondData);

            File.ReadAllBytes(BackupPath).Should().Equal(firstGeneration);
            File.ReadAllBytes(StoragePath).Should().NotEqual(firstGeneration);
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSavedOverExistingFile_ThenBackupIsLoadable()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);

            var (secondSections, secondData) = CreateSample(2);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, secondSections, secondData);

            var sections = CreateSections();
            var loaded = new Dictionary<string, Record>();
            BinaryStorageIO.LoadDataFromDisk(BackupPath, sections, loaded, KeyLoadFailedBehaviour.ThrowException);

            loaded.Should().ContainKey("value");
            ((Record<int>)loaded["value"]).Value.Should().Be(1);
        }

        [Test]
        public void WhenSavedManyTimes_ThenBackupAlwaysHoldsOneGenerationBack()
        {
            for (var generation = 1; generation <= 5; generation++)
            {
                var (sections, data) = CreateSample(generation);
                BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data);
            }

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Get<int>("value").Should().Be(5);

            var backupSections = CreateSections();
            var backup = new Dictionary<string, Record>();
            BinaryStorageIO.LoadDataFromDisk(BackupPath, backupSections, backup, KeyLoadFailedBehaviour.ThrowException);
            ((Record<int>)backup["value"]).Value.Should().Be(4);
        }

        [Test]
        public void WhenSavedRepeatedly_ThenStorageFileIsNeverMissing()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);

            var observer = new FileExistenceObserver(StoragePath, BackupPath);
            observer.Start();
            for (var generation = 2; generation <= 201; generation++)
            {
                var (sections, data) = CreateSample(generation);
                BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data);
            }
            observer.Stop();

            observer.SawBothMissing.Should().BeFalse();
        }

        [Test]
        public void WhenAllDataRemoved_ThenBackupRemovedToo()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);
            var (secondSections, secondData) = CreateSample(2);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, secondSections, secondData);
            File.Exists(BackupPath).Should().BeTrue();

            BinaryStorageIO.SaveDataOnDisk(StoragePath, CreateSections(), new Dictionary<string, Record>());

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenStorageDeleted_ThenBackupAndTempDeletedToo()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);
            var (secondSections, secondData) = CreateSample(2);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, secondSections, secondData);
            File.WriteAllBytes(TempPath, new byte[] { 1, 2, 3 });

            BinaryStorage.Delete(StoragePath);

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenStaleTempExists_ThenSaveStillPublishes()
        {
            var (firstSections, firstData) = CreateSample(1);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, firstSections, firstData);
            File.WriteAllBytes(TempPath, new byte[] { 9, 9, 9 });

            var (sections, data) = CreateSample(2);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data);

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Get<int>("value").Should().Be(2);
            File.Exists(TempPath).Should().BeFalse();
        }

        private static List<BinarySection> CreateSections()
        {
            return new List<BinarySection> { new TypedBinarySection<int>(Int32Serializer.Shared) };
        }

        private static (List<BinarySection> sections, Dictionary<string, Record> data) CreateSample(int value)
        {
            var sections = CreateSections();
            var data = new Dictionary<string, Record> { { "value", new Record<int>(value, 0) } };
            sections[0].Count++;
            return (sections, data);
        }

        private class FileExistenceObserver
        {
            private readonly string _storagePath;
            private readonly string _backupPath;
            private readonly System.Threading.Thread _thread;
            private volatile bool _running;

            public bool SawBothMissing { get; private set; }

            public FileExistenceObserver(string storagePath, string backupPath)
            {
                _storagePath = storagePath;
                _backupPath = backupPath;
                _thread = new System.Threading.Thread(Observe) { IsBackground = true };
            }

            public void Start()
            {
                _running = true;
                _thread.Start();
            }

            public void Stop()
            {
                _running = false;
                _thread.Join();
            }

            private void Observe()
            {
                while (_running)
                {
                    if (!File.Exists(_storagePath) && !File.Exists(_backupPath))
                    {
                        SawBothMissing = true;
                        return;
                    }
                }
            }
        }
    }
}
