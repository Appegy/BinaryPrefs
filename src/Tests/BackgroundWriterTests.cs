using System.Collections.Generic;
using System.IO;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class BackgroundWriterTests : BaseStorageTests
    {
        [Test]
        public void WhenSaveCalled_ThenDataIsOnDiskWhenItReturns()
        {
            using (var storage = Open())
            {
                storage.Set("value", 42);
                storage.Save();

                ReadValueFromDisk().Should().Be(42);
            }
        }

        [Test]
        public void WhenManyChangesQueued_ThenDiskHoldsTheLastState()
        {
            using (var storage = Open(autoSave: true))
            {
                for (var i = 1; i <= 200; i++)
                {
                    storage.Set("value", i);
                }
                storage.Save();
            }

            ReadValueFromDisk().Should().Be(200);
        }

        [Test]
        public void WhenDisposedWithPendingChanges_ThenTheyReachDisk()
        {
            using (var storage = Open(autoSave: true))
            {
                storage.Set("value", 7);
            }

            ReadValueFromDisk().Should().Be(7);
        }

        [Test]
        public void WhenStorageEmptied_ThenFilesAreRemoved()
        {
            using (var storage = Open(autoSave: true))
            {
                storage.Set("value", 1);
                storage.Save();
                storage.Set("other", 2);
                storage.Save();

                File.Exists(BackupPath).Should().BeTrue();

                storage.RemoveAll();
                storage.Save();
            }

            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        [Test]
        public void WhenBackgroundWriterDisabled_ThenAutoSaveWritesBeforeSetReturns()
        {
            using var storage = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .EnableAutoSaveOnChange()
                .SaveOnBackgroundThread(false)
                .Build();

            storage.Set("value", 11);

            ReadValueFromDisk().Should().Be(11);
        }

        [Test]
        public void WhenBackgroundWriterEnabled_ThenRepeatedSavesKeepBackupOneGenerationBehind()
        {
            using (var storage = Open())
            {
                storage.Set("value", 1);
                storage.Save();
                storage.Set("value", 2);
                storage.Save();
            }

            ReadValueFromDisk().Should().Be(2);
            ReadValueFrom(BackupPath).Should().Be(1);
        }

        [Test]
        public void WhenReopenedAfterBackgroundSave_ThenDataSurvives()
        {
            using (var storage = Open(autoSave: true))
            {
                storage.Set("value", 99);
                storage.Set("text", "kept");
            }

            using var reopened = Open();

            reopened.Get<int>("value").Should().Be(99);
            reopened.Get<string>("text").Should().Be("kept");
        }

        private BinaryStorage Open(bool autoSave = false)
        {
            var builder = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes();
            if (autoSave)
            {
                builder = builder.EnableAutoSaveOnChange();
            }
            return builder.Build();
        }

        private int ReadValueFromDisk()
        {
            return ReadValueFrom(StoragePath);
        }

        private static int ReadValueFrom(string filePath)
        {
            var sections = new List<BinarySection> { new TypedBinarySection<int>(Int32Serializer.Shared) };
            var data = new Dictionary<string, Record>();
            StorageFormat.ReadFile(filePath, sections, data, KeyLoadFailedBehaviour.Ignore);
            return ((Record<int>)data["value"]).Value;
        }
    }
}
