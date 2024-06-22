using System;

namespace Appegy.Storage
{
    public class CantSupportCollectionOfException : Exception
    {
        public Type ItemType { get; }

        public CantSupportCollectionOfException(Type itemType)
            : base($"Add serializer for {itemType.FullName} using {nameof(BinaryStorage.Builder.AddTypeSerializer)} before adding support for List<{itemType.Name}>.")
        {
            ItemType = itemType;
        }
    }
}