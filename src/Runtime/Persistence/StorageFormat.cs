using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Appegy.Storage
{
    internal static class StorageFormat
    {
        private const string UnknownTypeName = "<unknown>";
        private const int ReadBufferSize = 16 * 1024;
        private const int MinimumRecordSize = 13;

        #region Write

        internal static void Write(BinaryWriter writer, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            var stream = writer.BaseStream;

            // #01 <---> Store package version at the start of the file
            writer.Write(PackageInfo.Version);

            // #02 <---> Reserve 8 bytes for future updates
            writer.Write(0L);

            // #03 <---> Store amount of used serializers
            writer.Write(sections.Count);
            for (var i = 0; i < sections.Count; i++)
            {
                // #04 <---> Write only name of serializer type
                var section = sections[i];
                writer.Write(section.Count > 0 ? section.TypeName : string.Empty);
            }

            // #05 <---> Store amount of records in storage
            writer.Write(data.Count);
            foreach (var entry in data)
            {
                // #06 <---> Write key
                writer.Write(entry.Key);

                // #07 <---> Write type index
                writer.Write(entry.Value.TypeIndex);

                // #08 <---> Keep space for size (will be calculated later)
                var sizePosition = stream.Position;
                writer.Write(0L);

                // #09 <---> Write value itself
                var valuePosition = stream.Position;
                sections[entry.Value.TypeIndex].WriteTo(writer, entry.Value);
                var endPosition = stream.Position;

                // #08 <---> Write real size of entry
                stream.Position = sizePosition;
                writer.Write(endPosition - valuePosition);
                stream.Position = endPosition;
            }
        }

        #endregion

        #region Read

        internal static void ReadFile(string storageFilePath, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            using var stream = new FileStream(storageFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, ReadBufferSize);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            var fileSections = ReadHeader(storageFilePath, sections, reader, out var recordCount);
            var fileLength = stream.Length;
            data.EnsureCapacity((int)Math.Min(recordCount, (fileLength - stream.Position) / MinimumRecordSize));

            for (var i = 0; i < recordCount; i++)
            {
                var header = ReadRecordHeader(storageFilePath, reader, fileLength);
                if (!TryReadRecord(storageFilePath, fileSections, data, reader, header, keyLoadFailedBehaviour))
                {
                    stream.Position = Math.Min(header.ValuePosition + header.Size, fileLength);
                }
            }
        }

        private static FileSection[] ReadHeader(string storageFilePath, IReadOnlyList<BinarySection> sections, BinaryReader reader, out int recordCount)
        {
            var stream = reader.BaseStream;
            try
            {
                // #01 <---> Read package version from the start of the file
                reader.ReadString();

                // #02 <---> Read and skip reserved 8 bytes
                reader.ReadInt64();

                // #03 <---> Read used serializers amount
                var serializersCount = reader.ReadInt32();
                if (serializersCount < 0 || serializersCount > stream.Length - stream.Position)
                {
                    throw new StorageFileCorruptedException(storageFilePath, $"Invalid serializer count {serializersCount}");
                }

                var fileSections = new FileSection[serializersCount];
                for (var i = 0; i < serializersCount; i++)
                {
                    // #04 <---> Read name of type in serializer
                    var typeName = reader.ReadString();
                    var sectionIndex = IndexOfSection(sections, typeName);
                    fileSections[i] = new FileSection(typeName, sectionIndex == -1 ? null : sections[sectionIndex], sectionIndex);
                }

                // #05 <---> Read amount of records in storage
                recordCount = reader.ReadInt32();
                if (recordCount < 0)
                {
                    throw new StorageFileCorruptedException(storageFilePath, $"Invalid record count {recordCount}");
                }
                return fileSections;
            }
            catch (EndOfStreamException e)
            {
                throw new StorageFileCorruptedException(storageFilePath, "Unexpected end of file while reading header", e);
            }
        }

        private static RecordHeader ReadRecordHeader(string storageFilePath, BinaryReader reader, long fileLength)
        {
            string key;
            int typeIndex;
            long size;
            try
            {
                // #06 <---> Read key
                key = reader.ReadString();

                // #07 <---> Read type index
                typeIndex = reader.ReadInt32();

                // #08 <---> Read real size of entry
                size = reader.ReadInt64();
            }
            catch (EndOfStreamException e)
            {
                throw new StorageFileCorruptedException(storageFilePath, "Unexpected end of file while reading record header", e);
            }

            var valuePosition = reader.BaseStream.Position;
            if (size < 0 || valuePosition + size > fileLength)
            {
                throw new StorageFileCorruptedException(storageFilePath, $"Entry '{key}' of {size}b at {valuePosition} runs past the end of a {fileLength}b file");
            }
            return new RecordHeader(key, typeIndex, size, valuePosition);
        }

        private static bool TryReadRecord(string storageFilePath, FileSection[] fileSections, Dictionary<string, Record> data, BinaryReader reader, in RecordHeader header, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            if (header.TypeIndex < 0 || header.TypeIndex >= fileSections.Length)
            {
                ReportFailedKey(header, UnknownTypeName, keyLoadFailedBehaviour, $"Type index {header.TypeIndex} is out of range");
                return false;
            }

            var fileSection = fileSections[header.TypeIndex];
            if (fileSection.Section == null)
            {
                ReportFailedKey(header, fileSection.TypeName, keyLoadFailedBehaviour, "Unregistered type serializer");
                return false;
            }

            // #09 <---> Read value from stream
            Record value;
            try
            {
                value = fileSection.Section.ReadFrom(reader, fileSection.SectionIndex);
            }
            catch (Exception e)
            {
                ReportFailedKey(header, fileSection.TypeName, keyLoadFailedBehaviour, "Failed to deserialize value", e);
                return false;
            }

            var readSize = reader.BaseStream.Position - header.ValuePosition;
            if (readSize != header.Size)
            {
                ReportFailedKey(header, fileSection.TypeName, keyLoadFailedBehaviour, $"Read more than expected ({readSize}b)");
                return false;
            }

            if (!data.TryAdd(header.Key, value))
            {
                throw new StorageFileCorruptedException(storageFilePath, $"Duplicate key '{header.Key}'");
            }

            fileSection.Section.Count++;
            return true;
        }

        private static void ReportFailedKey(in RecordHeader header, string typeName, KeyLoadFailedBehaviour keyLoadFailedBehaviour, string reason, Exception exception = null)
        {
            switch (keyLoadFailedBehaviour)
            {
                case KeyLoadFailedBehaviour.ThrowException:
                    throw new KeyLoadFailedException(header.Key, typeName, header.Size, reason, exception);
                case KeyLoadFailedBehaviour.Ignore:
                    break;
                case KeyLoadFailedBehaviour.IgnoreWithWarning:
                    Debug.LogWarning($"Failed to load key {header.Key} of type {typeName} with size {header.Size}b. Reason: {reason}");
                    break;
                default:
                    throw new UnexpectedEnumException(typeof(KeyLoadFailedBehaviour), keyLoadFailedBehaviour);
            }
        }

        private static int IndexOfSection(IReadOnlyList<BinarySection> sections, string typeName)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i].TypeName == typeName)
                {
                    return i;
                }
            }
            for (var i = 0; i < sections.Count; i++)
            {
                var fallbackNames = sections[i].FallbackNames;
                for (var j = 0; j < fallbackNames.Count; j++)
                {
                    if (fallbackNames[j] == typeName)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private readonly struct FileSection
        {
            public readonly string TypeName;
            public readonly BinarySection Section;
            public readonly int SectionIndex;

            public FileSection(string typeName, BinarySection section, int sectionIndex)
            {
                TypeName = typeName;
                Section = section;
                SectionIndex = sectionIndex;
            }
        }

        private readonly struct RecordHeader
        {
            public readonly string Key;
            public readonly int TypeIndex;
            public readonly long Size;
            public readonly long ValuePosition;

            public RecordHeader(string key, int typeIndex, long size, long valuePosition)
            {
                Key = key;
                TypeIndex = typeIndex;
                Size = size;
                ValuePosition = valuePosition;
            }
        }

        #endregion
    }
}
