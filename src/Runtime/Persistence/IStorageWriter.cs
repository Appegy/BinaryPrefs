namespace Appegy.Storage
{
    /// <summary> Puts serialized storage snapshots on disk. Takes ownership of every snapshot handed to it and releases it once it is written or dropped. </summary>
    internal interface IStorageWriter
    {
        /// <summary>
        /// Hand a snapshot over to the writer. With <paramref name="waitForDisk"/> the bytes have reached the disk by the
        /// time this returns and an I/O failure is thrown to the caller; without it the failure is logged instead.
        /// </summary>
        void Write(StorageSnapshot snapshot, bool waitForDisk);

        /// <summary> Put whatever is still waiting on disk and return once nothing is in flight. </summary>
        void Flush();
    }
}
