using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class BaseStorageTests
    {
        protected readonly string StoragePath = Path.Combine(Application.temporaryCachePath, "test.bin");

        protected string TempPath => StoragePath + BinaryStorageIO.TempFileExtension;
        protected string BackupPath => StoragePath + BinaryStorageIO.BackupFileExtension;
        protected string CorruptedPath => StoragePath + BinaryStorageIO.CorruptedFileExtension;

        [SetUp, TearDown]
        public void CleanStorageBetweenTests()
        {
            DeleteIfExists(StoragePath);
            DeleteIfExists(TempPath);
            DeleteIfExists(BackupPath);
            DeleteIfExists(CorruptedPath);
        }

        private static void DeleteIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
