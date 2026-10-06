namespace Appegy.Storage
{
    /// <summary>
    /// Specifies the behavior when neither the storage file nor its backup can be read on load.
    /// </summary>
    public enum CorruptedFileBehaviour
    {
        /// <summary>
        /// Throws a <see cref="StorageFileCorruptedException"/> from <c>Build</c>.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Starts with an empty storage.
        /// </summary>
        ResetToEmpty,

        /// <summary>
        /// Starts with an empty storage and logs the exception.
        /// </summary>
        ResetToEmptyWithError,
    }
}
