using System;
using System.IO;
using System.Text;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class CorruptionReasonTests : BaseStorageTests
    {
        [Test]
        public void WhenEnumValuesChecked_ThenNumberingIsStable()
        {
            ((int)StorageCorruptionReason.None).Should().Be(0);
            ((int)StorageCorruptionReason.HeaderTruncated).Should().Be(1);
            ((int)StorageCorruptionReason.RecordHeaderTruncated).Should().Be(2);
            ((int)StorageCorruptionReason.InvalidSerializerCount).Should().Be(3);
            ((int)StorageCorruptionReason.InvalidRecordCount).Should().Be(4);
            ((int)StorageCorruptionReason.DuplicateKey).Should().Be(5);
            ((int)StorageCorruptionReason.EntrySizeOverflow).Should().Be(6);

            ((int)StorageLoadSource.Main).Should().Be(0);
            ((int)StorageLoadSource.Backup).Should().Be(1);
            ((int)StorageLoadSource.Tmp).Should().Be(2);
            ((int)StorageLoadSource.Empty).Should().Be(3);
        }

        [Test]
        public void WhenFileIsEmpty_ThenHeaderTruncated()
        {
            File.WriteAllBytes(StoragePath, Array.Empty<byte>());

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.HeaderTruncated);
        }

        [Test]
        public void WhenHeaderCutShort_ThenHeaderTruncated()
        {
            WriteFile(writer => writer.Write(PackageInfo.Version));

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.HeaderTruncated);
        }

        [Test]
        public void WhenSerializerCountNegative_ThenInvalidSerializerCount()
        {
            WriteFile(writer =>
            {
                WriteHeaderStart(writer);
                writer.Write(-1);
            });

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.InvalidSerializerCount);
        }

        [Test]
        public void WhenRecordCountNegative_ThenInvalidRecordCount()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(-1);
            });

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.InvalidRecordCount);
        }

        [Test]
        public void WhenRecordHeaderCutShort_ThenRecordHeaderTruncated()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(1);
            });

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.RecordHeaderTruncated);
        }

        [Test]
        public void WhenEntrySizeRunsPastEndOfFile_ThenEntrySizeOverflow()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(1);
                writer.Write("a");
                writer.Write(0);
                writer.Write(long.MaxValue / 2);
                writer.Write(1);
            });

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.EntrySizeOverflow);
        }

        [Test]
        public void WhenKeyDuplicated_ThenDuplicateKey()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(2);
                WriteIntRecord(writer, "a", 1);
                WriteIntRecord(writer, "a", 2);
            });

            Load().Should().Throw<StorageFileCorruptedException>()
                .Which.Reason.Should().Be(StorageCorruptionReason.DuplicateKey);
        }

        [Test]
        public void WhenCorrupted_ThenExceptionCarriesFileFacts()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(3);
                WriteIntRecord(writer, "a", 1);
                WriteIntRecord(writer, "a", 2);
            });
            var length = new FileInfo(StoragePath).Length;

            var exception = Load().Should().Throw<StorageFileCorruptedException>().Which;

            exception.FilePath.Should().Be(StoragePath);
            exception.FileLength.Should().Be(length);
            exception.RecordsExpected.Should().Be(3);
            exception.RecordsRecovered.Should().Be(1);
            exception.BytesConsumed.Should().BeGreaterThan(0);
        }

        [Test]
        public void WhenFileIsHealthy_ThenReportIsClean()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Set("b", "two");
                storage.Save();
            }

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            reopened.LoadReport.IsClean.Should().BeTrue();
            reopened.LoadReport.Reason.Should().Be(StorageCorruptionReason.None);
            reopened.LoadReport.Source.Should().Be(StorageLoadSource.Main);
            reopened.LoadReport.RecordsExpected.Should().Be(2);
            reopened.LoadReport.RecordsLoaded.Should().Be(2);
            reopened.LoadReport.KeysFailed.Should().Be(0);
            reopened.LoadReport.FileLength.Should().Be(new FileInfo(StoragePath).Length);
        }

        [Test]
        public void WhenNoFileOnDisk_ThenReportSaysEmpty()
        {
            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            storage.LoadReport.Source.Should().Be(StorageLoadSource.Empty);
            storage.LoadReport.IsClean.Should().BeTrue();
        }

        [Test]
        public void WhenKeySkipped_ThenReportCountsIt()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(2);
                WriteIntRecord(writer, "a", 1);
                writer.Write("b");
                writer.Write(7);
                writer.Write((long)sizeof(int));
                writer.Write(2);
            });

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            storage.LoadReport.IsClean.Should().BeFalse();
            storage.LoadReport.Reason.Should().Be(StorageCorruptionReason.None);
            storage.LoadReport.RecordsExpected.Should().Be(2);
            storage.LoadReport.RecordsLoaded.Should().Be(1);
            storage.LoadReport.KeysFailed.Should().Be(1);
        }

        [Test]
        public void WhenTailIsZeroed_ThenReportIsNotClean()
        {
            WriteFile(writer =>
            {
                WriteSections(writer);
                writer.Write(2);
                WriteIntRecord(writer, "a", 1);
                writer.Write(new byte[16]);
            });

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            storage.LoadReport.IsClean.Should().BeFalse();
            storage.Has("a").Should().BeTrue();
        }

        private Action Load()
        {
            return () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore).Dispose();
        }

        private void WriteFile(Action<BinaryWriter> write)
        {
            using var stream = new FileStream(StoragePath, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            write(writer);
        }

        private static void WriteHeaderStart(BinaryWriter writer)
        {
            writer.Write(PackageInfo.Version);
            writer.Write(0L);
        }

        private static void WriteSections(BinaryWriter writer)
        {
            WriteHeaderStart(writer);
            writer.Write(1);
            writer.Write(Int32Serializer.Shared.TypeName);
        }

        private static void WriteIntRecord(BinaryWriter writer, string key, int value)
        {
            writer.Write(key);
            writer.Write(0);
            writer.Write((long)sizeof(int));
            writer.Write(value);
        }
    }
}
