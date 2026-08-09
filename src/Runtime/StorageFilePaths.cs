namespace Appegy.Storage
{
    /// <summary> The storage file and the companion files kept next to it. Built once per storage so the paths are not rebuilt on every save. </summary>
    internal readonly struct StorageFilePaths
    {
        public readonly string Main;
        public readonly string Temp;
        public readonly string Backup;

        public StorageFilePaths(string mainFilePath)
        {
            Main = mainFilePath;
            Temp = mainFilePath + BinaryStorageIO.TempFileExtension;
            Backup = mainFilePath + BinaryStorageIO.BackupFileExtension;
        }

        public static implicit operator StorageFilePaths(string mainFilePath)
        {
            return new StorageFilePaths(mainFilePath);
        }
    }
}
