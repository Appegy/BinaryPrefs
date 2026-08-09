namespace Appegy.Storage
{
    /// <summary>
    /// Specifies the behavior when the type of a value associated with a key does not match the expected type.
    /// </summary>
    public enum TypeMismatchBehaviour
    {
        /// <summary>
        /// Throws an exception if there is a type mismatch.
        /// </summary>
        ThrowException,

        /// <summary>
        /// Overrides the existing value and type with the new value and type.
        /// </summary>
        OverrideValueAndType,

        /// <summary>
        /// Ignores the new value and type if there is a type mismatch.
        /// </summary>
        Ignore
    }
}