using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Appegy.Storage
{
    public class CorruptedFileBehaviourTests : BaseStorageTests
    {
        private static readonly byte[] NegativeStringLength = { 0xFF, 0xFF, 0xFF, 0xFF, 0x0F };

        private static readonly string[] Damages =
        {
            "EmptyFile",
            "CutHeader",
            "CutRecord",
            "GarbageTypeNameLength",
            "GarbageKeyLength",
            "NegativeRecordSize",
            "TooLargeRecordSize",
        };

        private static readonly CorruptedFileBehaviour[] Behaviours =
        {
            CorruptedFileBehaviour.ThrowException,
            CorruptedFileBehaviour.ResetToEmpty,
            CorruptedFileBehaviour.ResetToEmptyWithError,
        };

        [TestCaseSource(nameof(Damages))]
        public void WhenStorageIsDamaged_ThenDataIsRecoveredFromBackup(string damage)
        {
            var healthy = Healthy();
            File.WriteAllBytes(BackupPath, healthy);
            File.WriteAllBytes(StoragePath, Damaged(damage, healthy));

            using var storage = Open(corruptedFileBehaviour: CorruptedFileBehaviour.ThrowException);

            storage.Get<int>("generation").Should().Be(1);
        }

        [TestCaseSource(nameof(Damages))]
        public void WhenStorageIsDamagedWithoutBackup_AndThrow_ThenThrowsCorruptedAndRemovesFile(string damage)
        {
            File.WriteAllBytes(StoragePath, Damaged(damage, Healthy()));

            FluentActions.Invoking(() => Open(corruptedFileBehaviour: CorruptedFileBehaviour.ThrowException).Dispose()).Should().Throw<StorageFileCorruptedException>();

            File.Exists(StoragePath).Should().BeFalse();
        }

        [Test]
        public void WhenStorageAndBackupAreDamaged_AndThrow_ThenThrowsAndLeavesNoDamagedFile()
        {
            PutDamagedStorageAndBackup();

            FluentActions.Invoking(() => Open(corruptedFileBehaviour: CorruptedFileBehaviour.ThrowException).Dispose()).Should().Throw<StorageFileCorruptedException>();

            ShouldLeaveNoDamagedFile();
        }

        [Test]
        public void WhenStorageAndBackupAreDamaged_AndResetToEmpty_ThenBuildsEmptyAndLeavesNoDamagedFile()
        {
            PutDamagedStorageAndBackup();

            using (var storage = Open(corruptedFileBehaviour: CorruptedFileBehaviour.ResetToEmpty))
            {
                storage.Keys.Count.Should().Be(0);
            }

            LogAssert.NoUnexpectedReceived();
            ShouldLeaveNoDamagedFile();
        }

        [Test]
        public void WhenStorageAndBackupAreDamaged_AndResetToEmptyWithError_ThenBuildsEmptyLogsOnceAndLeavesNoDamagedFile()
        {
            PutDamagedStorageAndBackup();

            LogAssert.Expect(LogType.Exception, new Regex("invalid string length"));
            using (var storage = Open(corruptedFileBehaviour: CorruptedFileBehaviour.ResetToEmptyWithError))
            {
                storage.Keys.Count.Should().Be(0);
            }

            LogAssert.NoUnexpectedReceived();
            ShouldLeaveNoDamagedFile();
        }

        [Test]
        public void WhenBehaviourIsNotSet_ThenDefaultsToResetToEmptyWithError()
        {
            PutDamagedStorageAndBackup();

            LogAssert.Expect(LogType.Exception, new Regex("invalid string length"));
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build())
            {
                storage.Keys.Count.Should().Be(0);
            }

            LogAssert.NoUnexpectedReceived();
            ShouldLeaveNoDamagedFile();
        }

        [Test]
        public void WhenStorageCannotBeOpened_ThenErrorReachesCallerAndFileIsKept([ValueSource(nameof(Behaviours))] CorruptedFileBehaviour behaviour)
        {
            File.WriteAllBytes(StoragePath, Healthy());

            using (new FileStream(StoragePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                FluentActions.Invoking(() => Open(corruptedFileBehaviour: behaviour).Dispose()).Should().Throw<IOException>();
            }

            using var storage = Open(corruptedFileBehaviour: CorruptedFileBehaviour.ThrowException);
            storage.Get<int>("generation").Should().Be(1);
        }

        [Test]
        public void WhenKeyFailsToLoad_AndKeyLoadFailedThrows_ThenFileIsKept()
        {
            var bytes = WithUnknownType(Healthy());
            File.WriteAllBytes(StoragePath, bytes);

            FluentActions.Invoking(() => BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build(KeyLoadFailedBehaviour.ThrowException)).Should().Throw<KeyLoadFailedException>();

            File.ReadAllBytes(StoragePath).Should().Equal(bytes);
        }

        private byte[] Healthy()
        {
            using (var storage = Open())
            {
                storage.Set("generation", 1);
                storage.Save();
            }
            var bytes = File.ReadAllBytes(StoragePath);
            BinaryStorage.Delete(StoragePath);
            return bytes;
        }

        private void PutDamagedStorageAndBackup()
        {
            File.WriteAllBytes(StoragePath, Damaged("GarbageKeyLength", null));
            File.WriteAllBytes(BackupPath, Damaged("GarbageTypeNameLength", null));
        }

        private void ShouldLeaveNoDamagedFile()
        {
            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(BackupPath).Should().BeFalse();
        }

        private static byte[] Damaged(string damage, byte[] healthy)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            switch (damage)
            {
                case "EmptyFile":
                    break;
                case "CutHeader":
                    writer.Write(healthy[..3]);
                    break;
                case "CutRecord":
                    writer.Write(healthy[..^2]);
                    break;
                case "GarbageTypeNameLength":
                    writer.Write(PackageInfo.Version);
                    writer.Write(0L);
                    writer.Write(1);
                    writer.Write(NegativeStringLength);
                    break;
                case "GarbageKeyLength":
                    WriteHeader(writer);
                    writer.Write(NegativeStringLength);
                    break;
                case "NegativeRecordSize":
                    WriteHeader(writer);
                    writer.Write("generation");
                    writer.Write(0);
                    writer.Write(-8L);
                    writer.Write(1);
                    break;
                case "TooLargeRecordSize":
                    WriteHeader(writer);
                    writer.Write("generation");
                    writer.Write(0);
                    writer.Write(long.MaxValue / 2);
                    writer.Write(1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(damage), damage, null);
            }
            writer.Flush();
            return stream.ToArray();
        }

        private static void WriteHeader(BinaryWriter writer)
        {
            writer.Write(PackageInfo.Version);
            writer.Write(0L);
            writer.Write(1);
            writer.Write(Int32Serializer.Shared.TypeName);
            writer.Write(1);
        }

        private static byte[] WithUnknownType(byte[] healthy)
        {
            var bytes = (byte[])healthy.Clone();
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            using var writer = new BinaryWriter(stream, Encoding.UTF8);
            reader.ReadString();
            reader.ReadInt64();
            var serializersCount = reader.ReadInt32();
            for (var i = 0; i < serializersCount; i++)
            {
                reader.ReadString();
            }
            reader.ReadInt32();
            reader.ReadString();
            writer.Write(999);
            writer.Flush();
            return bytes;
        }
    }
}
