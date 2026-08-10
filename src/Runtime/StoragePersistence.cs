using System;
using System.Collections.Generic;
using System.IO;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage
{
    /// <summary> Everything a storage needs from the disk: where its records come from, where they go, and whether a save has to wait for them to get there. </summary>
    internal sealed class StoragePersistence
    {
        private readonly StorageFile _file;
        private readonly IReadOnlyList<BinarySection> _sections;
        private readonly StorageSerializer _serializer;
        private readonly IStorageWriter _writer;

        /// <summary> Whether a human-readable JSON copy is written next to the storage file on each save. </summary>
        public bool SaveJsonCopyForDebug { get; set; }

        public StoragePersistence(string filePath, IReadOnlyList<BinarySection> sections, bool saveOnBackgroundThread)
        {
            _file = StorageFile.Of(filePath);
            _sections = sections;
            _serializer = new StorageSerializer(sections);
            _writer = saveOnBackgroundThread ? new BackgroundStorageWriter(_file) : new ImmediateStorageWriter(_file);
        }

        /// <summary> Read the storage file into <paramref name="data"/>, falling back to the backup when it cannot be read. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        /// <exception cref="StorageFileCorruptedException"> Neither the storage file nor its backup could be read. Both are removed before this is thrown. </exception>
        /// <exception cref="KeyLoadFailedException"> A key failed to load and <paramref name="keyLoadFailedBehaviour"/> is <see cref="KeyLoadFailedBehaviour.ThrowException"/>. </exception>
        public void Load(Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            _file.Load(_sections, data, keyLoadFailedBehaviour);
        }

        /// <summary> Serialize <paramref name="data"/> on the calling thread and hand it to the writer. </summary>
        /// <param name="data"> Records to save </param>
        /// <param name="waitForDisk"> Whether to block until the bytes have reached the disk </param>
        /// <exception cref="IOException"> An I/O error occurred while <paramref name="waitForDisk"/> was requested </exception>
        public void Save(Dictionary<string, Record> data, bool waitForDisk)
        {
            _writer.Write(_serializer.Serialize(data, _file.NextGeneration()), waitForDisk);
            if (SaveJsonCopyForDebug)
            {
                SaveJsonCopy(data);
            }
        }

        /// <summary> Put whatever the writer still holds on disk. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        public void Flush()
        {
            _writer.Flush();
        }

        private void SaveJsonCopy(IReadOnlyDictionary<string, Record> data)
        {
            try
            {
                _file.WriteDebugJson(data.Count == 0 ? null : DebugJsonWriter.ToJson(data));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save JSON debug copy of '{_file.Main}'. Reason: {exception.Message}");
            }
        }
    }
}
