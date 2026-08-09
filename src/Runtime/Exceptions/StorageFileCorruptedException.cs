using System;

namespace Appegy.Storage
{
    public class StorageFileCorruptedException : Exception
    {
        /// <summary> Path of the file that could not be read. </summary>
        public string FilePath { get; }

        /// <summary> What exactly was wrong with the file. </summary>
        public StorageCorruptionReason Reason { get; }

        /// <summary> Size of the file, in bytes, or -1 when it could not be measured. </summary>
        public long FileLength { get; }

        /// <summary> How many records the header promised, or -1 when the header was not read. </summary>
        public int RecordsExpected { get; }

        /// <summary> How many records were read before the failure. </summary>
        public int RecordsRecovered { get; }

        /// <summary> Stream position at the moment of the failure. </summary>
        public long BytesConsumed { get; }

        public StorageFileCorruptedException(string filePath, StorageCorruptionReason reason, string details, long fileLength, int recordsExpected, int recordsRecovered, long bytesConsumed, Exception innerException = null)
            : base($"Storage file '{filePath}' is corrupted. Reason: {reason} ({details}). File {fileLength}b, {recordsRecovered}/{recordsExpected} records read, failed at byte {bytesConsumed}.", innerException)
        {
            FilePath = filePath;
            Reason = reason;
            FileLength = fileLength;
            RecordsExpected = recordsExpected;
            RecordsRecovered = recordsRecovered;
            BytesConsumed = bytesConsumed;
        }
    }
}
