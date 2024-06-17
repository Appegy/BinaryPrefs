using System;

namespace Appegy.BinaryStorage
{
    public class CollectionDisposedException : Exception
    {
        public CollectionDisposedException(string key)
            : base($"Collection related to {key} already disposed and can't be used anymore.")
        {
        }
    }
}