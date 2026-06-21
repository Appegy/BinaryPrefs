namespace Appegy.Storage
{
    /// <summary>
    /// Specifies the behavior when a requested key is not found in the storage.
    /// </summary>
    public enum MissingKeyBehavior
    {
        /// <summary>
        /// Initializes the key with the provided default value, or the type's non-null default when none is provided, and stores it.
        /// </summary>
        InitializeWithDefaultValue,

        /// <summary>
        /// Returns the provided default value, or the type's non-null default when none is provided, without storing it.
        /// </summary>
        ReturnDefaultValueOnly
    }
}