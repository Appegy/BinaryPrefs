using System;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class CorruptedLoadTests : BaseStorageTests
    {
        [Test]
        public void WhenTypeIndexOutOfRange_AndIgnore_ThenLoadDoesNotThrowAndKeySkipped()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Save();
            }

            CorruptFirstTypeIndex(StoragePath, 999);

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            reopened.Has("a").Should().BeFalse();
        }

        [Test]
        public void WhenTypeIndexOutOfRange_AndThrow_ThenBuildThrows()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Save();
            }

            CorruptFirstTypeIndex(StoragePath, 999);

            Action action = () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.ThrowException);

            action.Should().Throw<KeyLoadFailedException>();
        }

        [Test]
        public void WhenOneKeyCorrupted_AndIgnore_ThenOtherKeysStillLoad()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Set("b", 2);
                storage.Save();
            }

            CorruptFirstTypeIndex(StoragePath, 999);

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            reopened.Keys.Count.Should().Be(1);
        }

        private static void CorruptFirstTypeIndex(string path, int badValue)
        {
            var offset = FindFirstTypeIndexOffset(path);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            stream.Position = offset;
            writer.Write(badValue);
        }

        private static long FindFirstTypeIndexOffset(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            reader.ReadString();
            reader.ReadInt64();
            var serializersCount = reader.ReadInt32();
            for (var i = 0; i < serializersCount; i++)
            {
                reader.ReadString();
            }
            reader.ReadInt32();
            reader.ReadString();
            return stream.Position;
        }
    }
}
