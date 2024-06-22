using System;

namespace Appegy.BinaryStorage
{
    public class CollectionDisposedException : Exception
    {
        public CollectionDisposedException()
            : base("Collection already disposed and can't be used anymore.")
        {
        }
    }
}