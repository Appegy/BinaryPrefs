using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class BaseStorageTests
    {
        protected readonly string StoragePath = Path.Combine(Application.temporaryCachePath, "test.bin");

        [SetUp, TearDown]
        public void CleanStorageBetweenTests()
        {
            if (File.Exists(StoragePath))
            {
                File.Delete(StoragePath);
            }
        }
    }
}