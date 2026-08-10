using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using UnityEngine;

namespace Appegy.Storage
{
    internal static class BinaryStorageIO
    {
        internal const string TempFileExtension = ".tmp";
        internal const string BackupFileExtension = ".bak";

        private const string UnknownTypeName = "<unknown>";
        private const int ReadBufferSize = 16 * 1024;
        private const int MinimumRecordSize = 13;

        [ThreadStatic] private static PooledMemoryStream _serializationStream;
        [ThreadStatic] private static BinaryWriter _serializationWriter;

        #region Save

        /// <summary> Save data from memory to disk. </summary>
        /// <param name="paths"> Storage file and its companion files </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        internal static void SaveDataOnDisk(in StorageFilePaths paths, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            if (data.Count == 0)
            {
                DeleteFileIfExists(paths.Main);
                DeleteFileIfExists(paths.Temp);
                DeleteFileIfExists(paths.Backup);
                return;
            }

            EnsureDirectoryExists(paths.Temp);

            var buffer = SerializeToBuffer(sections, data);
            try
            {
                using var stream = new FileStream(paths.Temp, FileMode.Create);
                stream.Write(buffer.GetBuffer(), 0, (int)buffer.Length);
                stream.Flush(true);
            }
            finally
            {
                buffer.Release();
            }

            if (File.Exists(paths.Main))
            {
                File.Replace(paths.Temp, paths.Main, paths.Backup);
            }
            else
            {
                File.Move(paths.Temp, paths.Main);
            }
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
                // #01 <---> Store package version at the start of the file
                _serializationWriter.Write(PackageInfo.Version);

                // #02 <---> Reserve 8 bytes for future updates
                _serializationWriter.Write(0L);

                // #03 <---> Store amount of used serializers
                _serializationWriter.Write(sections.Count);
                for (var i = 0; i < sections.Count; i++)
                {
                    // #04 <---> Write only name of serializer type
                    var section = sections[i];
                    _serializationWriter.Write(section.Count > 0 ? section.TypeName : string.Empty);
                }

                // #05 <---> Store amount of records in storage
                _serializationWriter.Write(data.Count);
                foreach (var entry in data)
                {
                    // #06 <---> Write key
                    _serializationWriter.Write(entry.Key);

                    // #07 <---> Write type index
                    _serializationWriter.Write(entry.Value.TypeIndex);

                    // #08 <---> Keep space for size (will be calculated later)
                    var sizePosition = stream.Position;
                    _serializationWriter.Write(0L);

                    // #09 <---> Write value itself
                    var valuePosition = stream.Position;
                    sections[entry.Value.TypeIndex].WriteTo(_serializationWriter, entry.Value);
                    var endPosition = stream.Position;

                    // #08 <---> Write real size of entry
                    stream.Position = sizePosition;
                    _serializationWriter.Write(endPosition - valuePosition);
                    stream.Position = endPosition;
                }
            }
            catch
            {
                stream.Release();
                throw;
            }
            return stream;
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

            EnsureDirectoryExists(jsonFilePath);
            File.WriteAllText(jsonFilePath, DebugJsonWriter.ToJson(data), new UTF8Encoding(false));
        }

        #endregion

        #region Load

        /// <summary>
        /// Load data from disk to memory. When the storage file cannot be read, it is deleted and the backup written by the previous save takes its place.
        /// </summary>
        /// <param name="paths"> Storage file and its companion files </param>
        /// <param name="sections"> List of sections </param>
        /// <param name="data"> Dictionary to store data </param>
        /// <param name="keyLoadFailedBehaviour">Specify behaviour for broken keys</param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        /// <exception cref="StorageFileCorruptedException"> Neither the storage file nor its backup could be read. Both are removed before this is thrown. </exception>
        /// <exception cref="KeyLoadFailedException"> A key failed to load and <paramref name="keyLoadFailedBehaviour"/> is <see cref="KeyLoadFailedBehaviour.ThrowException"/>. </exception>
        internal static void LoadDataFromDisk(in StorageFilePaths paths, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            ResetData(sections, data);

            if (!File.Exists(paths.Main))
            {
                return;
            }

            DeleteFileIfExists(paths.Temp);

            if (TryReadFile(paths.Main, sections, data, keyLoadFailedBehaviour, out var storageFailure))
            {
                return;
            }

            DeleteFileIfExists(paths.Main);

            if (File.Exists(paths.Backup))
            {
                File.Move(paths.Backup, paths.Main);
                if (TryReadFile(paths.Main, sections, data, keyLoadFailedBehaviour, out _))
                {
                    return;
                }
                DeleteFileIfExists(paths.Main);
            }

            ExceptionDispatchInfo.Capture(storageFailure).Throw();
        }

        private static bool TryReadFile(string storageFilePath, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour, out StorageFileCorruptedException failure)
        {
            try
            {
                ReadFile(storageFilePath, sections, data, keyLoadFailedBehaviour);
                failure = null;
                return true;
            }
            catch (StorageFileCorruptedException exception)
            {
                ResetData(sections, data);
                failure = exception;
                return false;
            }
        }

        /// <summary> Read a single storage file into memory. Unlike <see cref="LoadDataFromDisk"/> this never touches any other file. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        /// <exception cref="StorageFileCorruptedException"> The file structure is corrupted (bad header, truncated framing, or a duplicate key). </exception>
        /// <exception cref="KeyLoadFailedException"> A key failed to load and <paramref name="keyLoadFailedBehaviour"/> is <see cref="KeyLoadFailedBehaviour.ThrowException"/>. </exception>
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
                    var section = FindSection(sections, typeName);
                    fileSections[i] = new FileSection(typeName, section, IndexOfSection(sections, section));
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

        private static void ResetData(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            data.Clear();
            for (var i = 0; i < sections.Count; i++)
            {
                sections[i].Count = 0;
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

        private static void EnsureDirectoryExists(string filePath)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
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
