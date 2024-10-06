using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Appegy.Storage
{
    internal static class BinaryStorageIO
    {
        /// <summary> Save data from memory to disk. </summary>
        /// <param name="storageFilePath"> Path to the storage file </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        internal static void SaveDataOnDisk(string storageFilePath, IReadOnlyList<BinarySection> sections, IReadOnlyDictionary<string, Record> data)
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

            using (var stream = new FileStream(storageFilePathTmp, FileMode.Create))
            {
                using var writer = new BinaryWriter(stream, Encoding.UTF8);

                // #01 <---> Store package version at the start of the file
                writer.Write(PackageInfo.Version);

                // #02 <---> Reserve 8 bytes for future updates
                writer.Write(0L);

                // #03 <---> Store amount of used serializers
                writer.Write(sections.Count);
                foreach (var section in sections)
                {
                    // #04 <---> Write only name of serializer type
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

            if (File.Exists(storageFilePath))
            {
                File.Delete(storageFilePath);
            }
            File.Move(storageFilePathTmp, storageFilePath);
        }

        /// <summary> Load data from disk to memory. </summary>
        /// <param name="storageFilePath"> Path to the storage file </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <param name="keyLoadFailedBehaviour">Specify behaviour for broken keys</param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
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

            // #01 <---> Read package version from the start of the file
            reader.ReadString();

            // #02 <---> Read and skip reserved 8 bytes
            reader.ReadInt64();

            // #03 <---> Read used serializers amount
            var serializersCount = reader.ReadInt32();
            var orderedSectionsFromFile = new BinarySection[serializersCount];
            var sectionsNames = new string[serializersCount];

            for (var i = 0; i < serializersCount; i++)
            {
                // #04 <---> Read name of type in serializer
                var serializerName = reader.ReadString();
                sectionsNames[i] = serializerName;
                var serializer = sections.FirstOrDefault(c => c.TypeName == serializerName);
                // orderedSectionsFromFile[i] = serializer ?? throw new UnregisteredTypeException(serializerName);
                orderedSectionsFromFile[i] = serializer;
            }

            // #05 <---> Read amount of records in storage
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                // #06 <---> Read key
                var key = reader.ReadString();

                // #07 <---> Read type index
                var typeIndex = reader.ReadInt32();

                // #08 <---> Read real size of entry
                var entrySize = reader.ReadInt64();
                var position = stream.Position;

                // #09 <---> Read value from stream
                var section = orderedSectionsFromFile[typeIndex];
                var index = sections.FindIndex(c => c == section);
                if (section == null || index == -1)
                {
                    failedToLoadKey("Unregistered type serializer");
                    continue;
                }
                try
                {
                    var value = section.ReadFrom(reader, index);
                    if (stream.Position != position + entrySize)
                    {
                        failedToLoadKey($"Read more than expected ({stream.Position - position}b)");
                    }
                    else
                    {
                        section.Count++;
                        data.Add(key, value);
                    }
                }
                catch (EndOfStreamException e)
                {
                    failedToLoadKey("End of file stream", e);
                }

                void failedToLoadKey(string reason, Exception exception = null)
                {
                    // move stream position to the next record
                    stream.Position = Math.Min(position + entrySize, stream.Length);

                    switch (keyLoadFailedBehaviour)
                    {
                        case KeyLoadFailedBehaviour.ThrowException:
                            throw exception ?? new KeyLoadFailedException(key, sectionsNames[typeIndex], entrySize, reason);
                        case KeyLoadFailedBehaviour.Ignore:
                            break;
                        case KeyLoadFailedBehaviour.IgnoreWithWarning:
                            Debug.LogWarning($"Failed to load key {key} of type {sectionsNames[typeIndex]} with size {entrySize}b. Reason: {reason}");
                            break;
                        default:
                            throw new UnexpectedEnumException(typeof(KeyLoadFailedBehaviour), keyLoadFailedBehaviour);
                    }
                }
            }
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