using System;
using System.IO;
using System.Text;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class TruncatedEntryTests : BaseStorageTests
    {
        [Test]
        public void WhenEntryRunsPastEndOfFile_ThenThrowsCorruptedEvenInIgnore()
        {
            WriteFile(writer =>
            {
                WriteHeader(writer, 1);
                writer.Write("a");
                writer.Write(0);
                writer.Write(long.MaxValue / 2);
                writer.Write(1);
            });

            Load().Should().Throw<StorageFileCorruptedException>();
        }

        [Test]
        public void WhenEntrySizeNegative_ThenThrowsCorruptedEvenInIgnore()
        {
            WriteFile(writer =>
            {
                WriteHeader(writer, 1);
                writer.Write("a");
                writer.Write(0);
                writer.Write(-8L);
                writer.Write(1);
            });

            Load().Should().Throw<StorageFileCorruptedException>();
        }

        [Test]
        public void WhenLastEntryTruncated_ThenThrowsInsteadOfSilentlyDroppingIt()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Set("b", "long enough to be cut");
                storage.Save();
            }
            var bytes = File.ReadAllBytes(StoragePath);
            File.WriteAllBytes(StoragePath, bytes[..(bytes.Length - 8)]);

            Load().Should().Throw<StorageFileCorruptedException>();
        }

        [Test]
        public void WhenFileIsHealthy_ThenEveryEntryLoads()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Set("b", "two");
                storage.Save();
            }

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            reopened.Get<int>("a").Should().Be(1);
            reopened.Get<string>("b").Should().Be("two");
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

        private static void WriteHeader(BinaryWriter writer, int recordCount)
        {
            writer.Write(PackageInfo.Version);
            writer.Write(0L);
            writer.Write(1);
            writer.Write(Int32Serializer.Shared.TypeName);
            writer.Write(recordCount);
        }
    }
}
