namespace Appegy.Storage
{
    public static class StorageExtensions
    {
        /// <summary>
        /// Creates a nested storage that isolates keys using the specified prefix.
        /// </summary>
        public static IBinaryStorage CreateChild(this IBinaryStorage root, string prefix)
        {
            return new NestedBinaryStorage(root, prefix);
        }
    }
}
