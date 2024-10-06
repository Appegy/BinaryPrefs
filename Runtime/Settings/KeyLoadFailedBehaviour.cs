namespace Appegy.Storage
{
    /// <summary>
    /// Specifies the behavior when a key fails to load from storage.
    /// </summary>
    public enum KeyLoadFailedBehaviour
    {
        /// <summary>
        /// Throws an exception if a key fails to load.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Ignores the failure and continue.
        /// </summary>
        Ignore,

        /// <summary>
        /// Ignores the failure and logs a warning.
        /// </summary>
        IgnoreWithWarning,
    }
}