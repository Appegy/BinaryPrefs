namespace Appegy.Storage
{
    /// <summary>
    /// Identifies which file on disk the loaded data came from.
    /// </summary>
    public enum StorageLoadSource
    {
        /// <summary> The storage file itself. </summary>
        Main = 0,

        /// <summary> The backup file holding the previous generation. </summary>
        Backup = 1,

        /// <summary> The temporary file of an unfinished save. </summary>
        Tmp = 2,

        /// <summary> Nothing was loaded - there was no readable file. </summary>
        Empty = 3,
    }
}
