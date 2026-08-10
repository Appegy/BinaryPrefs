using System.Collections.Concurrent;

namespace Appegy.Storage
{
    /// <summary> The storage file and the companion files kept next to it. Built once per storage so the paths are not rebuilt on every save. </summary>
    internal readonly struct StorageFilePaths
    {
        private static readonly ConcurrentDictionary<string, object> PublishLocks = new();

        public readonly string Main;
        public readonly string Temp;
        public readonly string Backup;

        /// <summary> Shared by every <see cref="StorageFilePaths"/> pointing at the same file, so that publishing it is serialized across threads and storages. </summary>
        public readonly object PublishLock;

        public StorageFilePaths(string mainFilePath)
        {
            Main = mainFilePath;
            Temp = mainFilePath + BinaryStorageIO.TempFileExtension;
            Backup = mainFilePath + BinaryStorageIO.BackupFileExtension;
            PublishLock = PublishLocks.GetOrAdd(mainFilePath, _ => new object());
        }

        public static implicit operator StorageFilePaths(string mainFilePath)
        {
            return new StorageFilePaths(mainFilePath);
        }
    }
}
