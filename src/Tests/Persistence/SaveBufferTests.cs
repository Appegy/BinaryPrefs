using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class SaveBufferTests : BaseStorageTests
    {
        private string LegacyStoragePath => StoragePath + ".legacy";

        [SetUp, TearDown]
        public void CleanLegacyFileBetweenTests()
        {
            if (File.Exists(LegacyStoragePath))
            {
                File.Delete(LegacyStoragePath);
            }
        }

        [Test]
        public void WhenDataSerializedIntoBuffer_ThenBytesAreSameAsBeforeRefactor()
        {
            var (sections, data) = CreateSample(3, 16);

            var expected = SerializeAsBeforeRefactor(sections, data);
            var actual = SerializeIntoBuffer(sections, data);

            actual.Should().Equal(expected);
        }

        [Test]
        public void WhenFileSaved_ThenBytesAreSameAsBeforeRefactor()
        {
            var (sections, data) = CreateSample(3, 16);
            File.WriteAllBytes(LegacyStoragePath, SerializeAsBeforeRefactor(sections, data));

            SaveOnDisk(StoragePath, sections, data);

            File.ReadAllBytes(StoragePath).Should().Equal(File.ReadAllBytes(LegacyStoragePath));
        }

        [Test]
        public void WhenDataExceedsInitialBuffer_ThenBytesAreSameAsBeforeRefactor()
        {
            var (sections, data) = CreateSample(64, 512);
            File.WriteAllBytes(LegacyStoragePath, SerializeAsBeforeRefactor(sections, data));

            SaveOnDisk(StoragePath, sections, data);

            File.ReadAllBytes(StoragePath).Should().Equal(File.ReadAllBytes(LegacyStoragePath));
        }

        [Test]
        public void WhenDataExceedsInitialBuffer_AndStorageReloaded_ThenAllValuesAreValid()
        {
            var (sections, data) = CreateSample(64, 512);

            SaveOnDisk(StoragePath, sections, data);

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Get<int>("int").Should().Be(42);
            storage.Get<string>("string_0").Should().Be(new string('x', 512));
            storage.Get<string>("string_63").Should().Be(new string('x', 512));
        }

        [Test]
        public void WhenSerializerThrows_ThenBufferReturnedToPool()
        {
            var (sections, data) = CreateThrowingSample();
            var serializer = new StorageSerializer(sections);

            Action action = () => serializer.Serialize(data);

            action.Should().Throw<InvalidOperationException>();
            serializer.BufferCapacity.Should().Be(0);
        }

        [Test]
        public void WhenSerializerThrows_ThenNoFilesLeftOnDisk()
        {
            var (sections, data) = CreateThrowingSample();

            Action action = () => SaveOnDisk(StoragePath, sections, data);

            action.Should().Throw<InvalidOperationException>();
            File.Exists(StoragePath).Should().BeFalse();
            File.Exists(TempPath).Should().BeFalse();
        }

        [Test]
        public void WhenSerializerThrew_ThenNextSaveWritesValidFile()
        {
            var (throwingSections, throwingData) = CreateThrowingSample();
            Action action = () => SaveOnDisk(StoragePath, throwingSections, throwingData);
            action.Should().Throw<InvalidOperationException>();

            var (sections, data) = CreateSample(3, 16);
            SaveOnDisk(StoragePath, sections, data);

            using var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().Build();
            storage.Get<int>("int").Should().Be(42);
            storage.Get<string>("string_2").Should().Be(new string('x', 16));
        }

        [Test]
        public void WhenStorageIsEmpty_ThenFileDeleted()
        {
            var sections = new List<BinarySection> { new TypedBinarySection<int>(Int32Serializer.Shared) };
            File.WriteAllBytes(StoragePath, new byte[] { 1, 2, 3 });

            SaveOnDisk(StoragePath, sections, new Dictionary<string, Record>());

            File.Exists(StoragePath).Should().BeFalse();
        }

        [Test]
        public void WhenBigStorageSavedBeforeSmallOne_ThenSmallOneIsStillCorrect()
        {
            var (bigSections, bigData) = CreateSample(64, 512);
            SaveOnDisk(StoragePath, bigSections, bigData);

            var (sections, data) = CreateSample(1, 8);
            SaveOnDisk(StoragePath, sections, data);

            File.ReadAllBytes(StoragePath).Should().Equal(SerializeAsBeforeRefactor(sections, data));
        }

        private static (List<BinarySection> sections, Dictionary<string, Record> data) CreateSample(int stringKeys, int stringLength)
        {
            var sections = new List<BinarySection>
            {
                new TypedBinarySection<int>(Int32Serializer.Shared),
                new TypedBinarySection<string>(StringSerializer.Shared),
                new TypedBinarySection<bool>(BooleanSerializer.Shared)
            };
            var data = new Dictionary<string, Record> { { "int", new Record<int>(42, 0) } };
            sections[0].Count++;
            var value = new string('x', stringLength);
            for (var i = 0; i < stringKeys; i++)
            {
                data.Add($"string_{i}", new Record<string>(value, 1));
                sections[1].Count++;
            }
            return (sections, data);
        }

        private static (List<BinarySection> sections, Dictionary<string, Record> data) CreateThrowingSample()
        {
            var sections = new List<BinarySection> { new TypedBinarySection<int>(new ThrowingSerializer()) };
            var data = new Dictionary<string, Record> { { "boom", new Record<int>(1, 0) } };
            sections[0].Count++;
            return (sections, data);
        }

        private static byte[] SerializeIntoBuffer(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            var snapshot = new StorageSerializer(sections).Serialize(data);
            var bytes = new byte[snapshot.Length];
            Array.Copy(snapshot.Buffer, bytes, bytes.Length);
            snapshot.Release();
            return bytes;
        }

        private static byte[] SerializeAsBeforeRefactor(IReadOnlyList<BinarySection> sections, IReadOnlyDictionary<string, Record> data)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(PackageInfo.Version);
                writer.Write(0L);

                writer.Write(sections.Count);
                foreach (var section in sections)
                {
                    writer.Write(section.Count > 0 ? section.TypeName : string.Empty);
                }

                writer.Write(data.Count);
                foreach (var entry in data)
                {
                    writer.Write(entry.Key);
                    writer.Write(entry.Value.TypeIndex);

                    var position = writer.BaseStream.Position;
                    writer.Write(0L);

                    var start = writer.BaseStream.Position;
                    var serializer = sections[entry.Value.TypeIndex];
                    serializer.WriteTo(writer, entry.Value);
                    var entrySize = writer.BaseStream.Position - start;

                    (position, writer.BaseStream.Position) = (writer.BaseStream.Position, position);
                    writer.Write(entrySize);
                    (_, writer.BaseStream.Position) = (writer.BaseStream.Position, position);
                }
            }
            return stream.ToArray();
        }

        private class ThrowingSerializer : TypeSerializer<int>
        {
            public override string TypeName => "throwing";
            public override bool Equals(int value1, int value2) => value1 == value2;
            public override void WriteTo(BinaryWriter writer, int value) => throw new InvalidOperationException("Serializer failed on purpose");
            public override int ReadFrom(BinaryReader reader) => reader.ReadInt32();
        }
    }
}
