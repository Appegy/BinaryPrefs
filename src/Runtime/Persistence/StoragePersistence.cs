using System;
using System.Collections.Generic;
using System.IO;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage
{
    internal sealed class StoragePersistence
    {
        private readonly StorageFile _file;
        private readonly StorageSerializer _serializer;
        private readonly IStorageWriter _writer;

        public bool SaveJsonCopyForDebug { get; set; }

        public StoragePersistence(string filePath, IReadOnlyList<BinarySection> sections, bool saveOnBackgroundThread)
        {
            _file = StorageFile.Of(filePath);
            _serializer = new StorageSerializer(sections);
            _writer = saveOnBackgroundThread ? new BackgroundStorageWriter(_file) : new ImmediateStorageWriter(_file);
        }

        public void Load(Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            _serializer.Clear(data);
            _file.Load((string filePath, out StorageFileCorruptedException failure) => _serializer.TryDeserialize(filePath, data, keyLoadFailedBehaviour, out failure));
        }

        public void Save(Dictionary<string, Record> data, bool waitForDisk)
        {
            _writer.Write(_serializer.Serialize(data), waitForDisk);
            if (SaveJsonCopyForDebug)
            {
                SaveJsonCopy(data);
            }
        }

        public void Flush()
        {
            _writer.Flush();
        }

        private void SaveJsonCopy(IReadOnlyDictionary<string, Record> data)
        {
            try
            {
                if (data.Count == 0)
                {
                    _file.RemoveDebugJson();
                }
                else
                {
                    _file.WriteDebugJson(DebugJsonWriter.ToJson(data));
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to save JSON debug copy of '{_file.Main}'. Reason: {exception.Message}");
            }
        }
    }
}
