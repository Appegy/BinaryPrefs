namespace Appegy.Storage
{
    public static class StorageExtensions
    {
        public static IBinaryStorage CreateChild(this IBinaryStorage root, string prefix)
        {
            return new NestedBinaryStorage(root, prefix);
        }
    }
}