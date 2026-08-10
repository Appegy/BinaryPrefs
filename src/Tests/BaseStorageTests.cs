using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class BaseStorageTests
    {
        protected readonly string StoragePath = Path.Combine(Application.temporaryCachePath, "test.bin");

        protected string TempPath => StoragePath + StorageFile.TempFileExtension;
        protected string BackupPath => StoragePath + StorageFile.BackupFileExtension;
        protected string JsonPath => StoragePath + StorageFile.DebugJsonFileExtension;

        [SetUp, TearDown]
        public void CleanStorageBetweenTests()
        {
            BinaryStorage.Delete(StoragePath);
        }

        /// <summary> Serializes and publishes the given records on the calling thread, the way a storage without a background writer does. </summary>
        internal static void SaveOnDisk(string filePath, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            new StoragePersistence(filePath, sections, false).Save(data, true);
        }
    }
}
