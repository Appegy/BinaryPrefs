namespace Appegy.Storage
{
    /// <summary>
    /// Describes how the storage file was read. Never null on a built storage.
    /// </summary>
    public class StorageLoadReport
    {
        public static readonly StorageLoadReport Empty = new(StorageLoadSource.Empty, StorageCorruptionReason.None, 0, 0, 0, 0);

        /// <summary> True when the file was read without structural problems and without failed keys. </summary>
        public bool IsClean => Reason == StorageCorruptionReason.None && KeysFailed == 0;

        /// <summary> Which file the data came from. </summary>
        public StorageLoadSource Source { get; }

        /// <summary> What was wrong with the file, or <see cref="StorageCorruptionReason.None"/>. </summary>
        public StorageCorruptionReason Reason { get; }

        /// <summary> Size of the file that was read, in bytes. </summary>
        public long FileLength { get; }

        /// <summary> How many records the header promised. </summary>
        public int RecordsExpected { get; }

        /// <summary> How many records ended up in memory. </summary>
        public int RecordsLoaded { get; }

        /// <summary> How many keys were skipped because their value could not be read. </summary>
        public int KeysFailed { get; }

        public StorageLoadReport(StorageLoadSource source, StorageCorruptionReason reason, long fileLength, int recordsExpected, int recordsLoaded, int keysFailed)
        {
            Source = source;
            Reason = reason;
            FileLength = fileLength;
            RecordsExpected = recordsExpected;
            RecordsLoaded = recordsLoaded;
            KeysFailed = keysFailed;
        }

        public override string ToString()
        {
            return $"{Source} source, {Reason} reason, {FileLength}b, {RecordsLoaded}/{RecordsExpected} records, {KeysFailed} keys failed";
        }
    }
}
