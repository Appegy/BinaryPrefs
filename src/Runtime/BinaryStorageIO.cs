using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Appegy.Storage
{
    internal static class BinaryStorageIO
    {
        [ThreadStatic] private static PooledMemoryStream _serializationStream;
        [ThreadStatic] private static BinaryWriter _serializationWriter;

        /// <summary> Save data from memory to disk. </summary>
        /// <param name="storageFilePath"> Path to the storage file </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        internal static void SaveDataOnDisk(string storageFilePath, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            // make sure there is no temp file from previous (most likely failed) save try
            var storageFilePathTmp = storageFilePath + ".tmp";
            DeleteFileIfExists(storageFilePathTmp);

            // delete storage if it exists when no data
            if (data.Count == 0)
            {
                DeleteFileIfExists(storageFilePath);
                return;
            }

            // prepare directory for save
            var directoryName = Path.GetDirectoryName(storageFilePathTmp);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            var buffer = SerializeToBuffer(sections, data);
            try
            {
                using var stream = new FileStream(storageFilePathTmp, FileMode.Create);
                stream.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
                stream.Flush(true);
            }
            finally
            {
                buffer.Release();
            }

            if (File.Exists(storageFilePath))
            {
                File.Delete(storageFilePath);
            }
            File.Move(storageFilePathTmp, storageFilePath);
        }

        /// <summary> Serialize data into a pooled in-memory buffer. Caller owns the returned stream and must call <see cref="PooledMemoryStream.Release"/>. </summary>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <returns> Stream holding the serialized bytes </returns>
        internal static PooledMemoryStream SerializeToBuffer(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            _serializationStream ??= new PooledMemoryStream();
            _serializationWriter ??= new BinaryWriter(_serializationStream, Encoding.UTF8);

            var stream = _serializationStream;
            stream.Reset();
            try
            {
                WriteData(_serializationWriter, sections, data);
            }
            catch
            {
                stream.Release();
                throw;
            }
            return stream;
        }

        private static void WriteData(BinaryWriter writer, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
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
                var position = writer.BaseStream.Position;
                writer.Write(0L);

                // #09 <---> Write value itself
                var start = writer.BaseStream.Position;
                var serializer = sections[entry.Value.TypeIndex];
                serializer.WriteTo(writer, entry.Value);
                var entrySize = writer.BaseStream.Position - start;

                // #08 <---> Write real size of entry
                (position, writer.BaseStream.Position) = (writer.BaseStream.Position, position);
                writer.Write(entrySize);
                (_, writer.BaseStream.Position) = (writer.BaseStream.Position, position);
            }
        }

        /// <summary> Load data from disk to memory. </summary>
        /// <param name="storageFilePath"> Path to the storage file </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <param name="keyLoadFailedBehaviour">Specify behaviour for broken keys</param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        /// <exception cref="StorageFileCorruptedException"> The file structure is corrupted (bad header, truncated framing, or a duplicate key). </exception>
        /// <exception cref="KeyLoadFailedException"> A key failed to load and <paramref name="keyLoadFailedBehaviour"/> is <see cref="KeyLoadFailedBehaviour.ThrowException"/>. </exception>
        internal static void LoadDataFromDisk(string storageFilePath, IReadOnlyList<BinarySection> sections, IDictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            data.Clear();
            foreach (var section in sections)
            {
                section.Count = 0;
            }
            if (!File.Exists(storageFilePath))
            {
                return;
            }
            using var stream = new FileStream(storageFilePath, FileMode.Open);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            int serializersCount;
            BinarySection[] orderedSectionsFromFile;
            string[] sectionsNames;
            int count;
            try
            {
                // #01 <---> Read package version from the start of the file
                reader.ReadString();

                // #02 <---> Read and skip reserved 8 bytes
                reader.ReadInt64();

                // #03 <---> Read used serializers amount
                serializersCount = reader.ReadInt32();
                if (serializersCount < 0 || serializersCount > stream.Length - stream.Position)
                {
                    throw new StorageFileCorruptedException(storageFilePath, $"Invalid serializer count {serializersCount}");
                }
                orderedSectionsFromFile = new BinarySection[serializersCount];
                sectionsNames = new string[serializersCount];

                for (var i = 0; i < serializersCount; i++)
                {
                    // #04 <---> Read name of type in serializer
                    var serializerName = reader.ReadString();
                    sectionsNames[i] = serializerName;
                    orderedSectionsFromFile[i] = FindSection(sections, serializerName);
                }

                // #05 <---> Read amount of records in storage
                count = reader.ReadInt32();
                if (count < 0)
                {
                    throw new StorageFileCorruptedException(storageFilePath, $"Invalid record count {count}");
                }
            }
            catch (EndOfStreamException e)
            {
                throw new StorageFileCorruptedException(storageFilePath, "Unexpected end of file while reading header", e);
            }

            for (var i = 0; i < count; i++)
            {
                string key;
                int typeIndex;
                long entrySize;
                try
                {
                    // #06 <---> Read key
                    key = reader.ReadString();

                    // #07 <---> Read type index
                    typeIndex = reader.ReadInt32();

                    // #08 <---> Read real size of entry
                    entrySize = reader.ReadInt64();
                }
                catch (EndOfStreamException e)
                {
                    throw new StorageFileCorruptedException(storageFilePath, "Unexpected end of file while reading record header", e);
                }
                var position = stream.Position;

                // #09 <---> Read value from stream
                if (typeIndex < 0 || typeIndex >= orderedSectionsFromFile.Length)
                {
                    failedToLoadKey($"Type index {typeIndex} is out of range");
                    continue;
                }
                var section = orderedSectionsFromFile[typeIndex];
                var index = IndexOfSection(sections, section);
                if (section == null || index == -1)
                {
                    failedToLoadKey("Unregistered type serializer");
                    continue;
                }

                Record value;
                try
                {
                    value = section.ReadFrom(reader, index);
                }
                catch (Exception e)
                {
                    failedToLoadKey("Failed to deserialize value", e);
                    continue;
                }

                if (stream.Position != position + entrySize)
                {
                    failedToLoadKey($"Read more than expected ({stream.Position - position}b)");
                    continue;
                }

                if (data.ContainsKey(key))
                {
                    throw new StorageFileCorruptedException(storageFilePath, $"Duplicate key '{key}'");
                }

                section.Count++;
                data.Add(key, value);

                void failedToLoadKey(string reason, Exception exception = null)
                {
                    // move stream position to the next record
                    stream.Position = Math.Min(position + entrySize, stream.Length);

                    var typeName = typeIndex >= 0 && typeIndex < sectionsNames.Length ? sectionsNames[typeIndex] : "<unknown>";
                    switch (keyLoadFailedBehaviour)
                    {
                        case KeyLoadFailedBehaviour.ThrowException:
                            throw new KeyLoadFailedException(key, typeName, entrySize, reason, exception);
                        case KeyLoadFailedBehaviour.Ignore:
                            break;
                        case KeyLoadFailedBehaviour.IgnoreWithWarning:
                            Debug.LogWarning($"Failed to load key {key} of type {typeName} with size {entrySize}b. Reason: {reason}");
                            break;
                        default:
                            throw new UnexpectedEnumException(typeof(KeyLoadFailedBehaviour), keyLoadFailedBehaviour);
                    }
                }
            }
        }

        private static BinarySection FindSection(IReadOnlyList<BinarySection> sections, string typeName)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i].TypeName == typeName)
                {
                    return sections[i];
                }
            }
            for (var i = 0; i < sections.Count; i++)
            {
                var fallbackNames = sections[i].FallbackNames;
                for (var j = 0; j < fallbackNames.Count; j++)
                {
                    if (fallbackNames[j] == typeName)
                    {
                        return sections[i];
                    }
                }
            }
            return null;
        }

        private static int IndexOfSection(IReadOnlyList<BinarySection> sections, BinarySection section)
        {
            for (var i = 0; i < sections.Count; i++)
            {
                if (sections[i] == section)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary> Save a human-readable JSON copy of the data next to the binary file. The copy is write-only and never loaded back. </summary>
        /// <param name="storageFilePath"> Path to the binary storage file </param>
        /// <param name="data"> Dictionary with the data </param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        internal static void SaveJsonCopyOnDisk(string storageFilePath, IReadOnlyDictionary<string, Record> data)
        {
            var jsonFilePath = storageFilePath + ".json";

            if (data.Count == 0)
            {
                DeleteFileIfExists(jsonFilePath);
                return;
            }

            var directoryName = Path.GetDirectoryName(jsonFilePath);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(jsonFilePath, DebugJsonWriter.ToJson(data), new UTF8Encoding(false));
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}