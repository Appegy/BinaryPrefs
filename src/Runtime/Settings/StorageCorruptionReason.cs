namespace Appegy.Storage
{
    /// <summary>
    /// Identifies what exactly was wrong with a storage file.
    /// </summary>
    public enum StorageCorruptionReason
    {
        /// <summary> File ended while the header was still being read. </summary>
        HeaderTruncated = 1,

        /// <summary> File ended while a record header was still being read. </summary>
        RecordHeaderTruncated = 2,

        /// <summary> Serializer count in the header is negative or larger than the rest of the file. </summary>
        InvalidSerializerCount = 3,

        /// <summary> Record count in the header is negative. </summary>
        InvalidRecordCount = 4,

        /// <summary> The same key appears twice. </summary>
        DuplicateKey = 5,

        /// <summary> A record claims to extend past the end of the file. </summary>
        EntrySizeOverflow = 6,
    }
}
