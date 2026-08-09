using System;

namespace Appegy.Storage
{
    public class StorageFileCorruptedException : Exception
    {
        public StorageFileCorruptedException(string filePath, string reason)
            : base($"Storage file '{filePath}' is corrupted. Reason: {reason}")
        {
        }

        public StorageFileCorruptedException(string filePath, string reason, Exception innerException)
            : base($"Storage file '{filePath}' is corrupted. Reason: {reason}", innerException)
        {
        }
    }
}
