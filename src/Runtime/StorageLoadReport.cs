namespace Appegy.Storage
{
    /// <summary>
    /// Describes how the storage file was read. Never null on a built storage.
    /// </summary>
    public class StorageLoadReport
    {
        public static readonly StorageLoadReport Empty = new(0, 0, 0, 0);

        /// <summary> True when every record the file promised ended up in memory. </summary>
        public bool IsClean => KeysFailed == 0;

        /// <summary> Size of the file that was read, in bytes. Zero when there was no file. </summary>
        public long FileLength { get; }

        /// <summary> How many records the header promised. </summary>
        public int RecordsExpected { get; }

        /// <summary> How many records ended up in memory. </summary>
        public int RecordsLoaded { get; }

        /// <summary> How many keys were skipped because their value could not be read. </summary>
        public int KeysFailed { get; }

        public StorageLoadReport(long fileLength, int recordsExpected, int recordsLoaded, int keysFailed)
        {
            FileLength = fileLength;
            RecordsExpected = recordsExpected;
            RecordsLoaded = recordsLoaded;
            KeysFailed = keysFailed;
        }

        public override string ToString()
        {
            return $"{FileLength}b, {RecordsLoaded}/{RecordsExpected} records, {KeysFailed} keys failed";
        }
    }
}
