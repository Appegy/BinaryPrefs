using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
        public void WhenTypeIndexOutOfRange_AndIgnoreWithWarning_ThenWarningLoggedAndKeySkipped()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Save();
            }

            CorruptFirstTypeIndex(StoragePath, 999);

            LogAssert.Expect(LogType.Warning, new Regex("Failed to load key a .*Type index 999 is out of range"));
            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.IgnoreWithWarning);

            reopened.Has("a").Should().BeFalse();
        }

        [Test]
        public void WhenBuiltWithoutArgument_ThenDefaultsToIgnoreWithWarning()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Save();
            }

            CorruptFirstTypeIndex(StoragePath, 999);

            LogAssert.Expect(LogType.Warning, new Regex("Failed to load key a .*Type index 999 is out of range"));
            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();

            reopened.Has("a").Should().BeFalse();
        }

        [Test]
        public void WhenOneKeyCorrupted_AndIgnore_ThenOtherKeyLoadsWithCorrectValue()
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
            if (reopened.Has("a"))
            {
                reopened.Get<int>("a").Should().Be(1);
            }
            else
            {
                reopened.Get<int>("b").Should().Be(2);
            }
        }

        [Test]
        public void WhenValueDeserializationFails_AndIgnore_ThenKeySkipped()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("k", "hello");
                storage.Save();
            }

            CorruptFirstStringValueLength(StoragePath, -2);

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            reopened.Has("k").Should().BeFalse();
        }

        [Test]
        public void WhenValueDeserializationFails_AndThrow_ThenThrowsKeyLoadFailedException()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("k", "hello");
                storage.Save();
            }

            CorruptFirstStringValueLength(StoragePath, -2);

            Action action = () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.ThrowException);

            action.Should().Throw<KeyLoadFailedException>();
        }

        [Test]
        public void WhenSerializerCountNegative_ThenThrowsCorruptedEvenInIgnore()
        {
            using (var stream = new FileStream(StoragePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write("1.0.0");
                writer.Write(0L);
                writer.Write(-1);
            }

            Action action = () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            action.Should().Throw<StorageFileCorruptedException>();
        }

        [Test]
        public void WhenHeaderTruncated_ThenThrowsCorruptedEvenInIgnore()
        {
            using (var stream = new FileStream(StoragePath, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write("1.0.0");
            }

            Action action = () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            action.Should().Throw<StorageFileCorruptedException>();
        }

        [Test]
        public void WhenDuplicateKey_ThenThrowsCorruptedEvenInIgnore()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Set("a", 1);
                storage.Save();
            }

            DuplicateFirstRecord(StoragePath);

            Action action = () => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.Ignore);

            action.Should().Throw<StorageFileCorruptedException>();
        }

        private static void CorruptFirstTypeIndex(string path, int badValue)
        {
            var offset = FindFirstTypeIndexOffset(path);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            stream.Position = offset;
            writer.Write(badValue);
        }

        private static void CorruptFirstStringValueLength(string path, int badLength)
        {
            var offset = FindFirstTypeIndexOffset(path) + sizeof(int) + sizeof(long);
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            stream.Position = offset;
            writer.Write(badLength);
        }

        private static void DuplicateFirstRecord(string path)
        {
            var countOffset = FindRecordCountOffset(path);
            var bytes = File.ReadAllBytes(path);
            var recordsStart = (int)countOffset + sizeof(int);
            var recordBlock = new byte[bytes.Length - recordsStart];
            Array.Copy(bytes, recordsStart, recordBlock, 0, recordBlock.Length);

            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            writer.Write(bytes, 0, (int)countOffset);
            writer.Write(2);
            writer.Write(recordBlock);
            writer.Write(recordBlock);
        }

        private static long FindRecordCountOffset(string path)
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
            return stream.Position;
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
