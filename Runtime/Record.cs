using System;

namespace Appegy.BinaryStorage
{
    internal abstract class Record
    {
        public abstract Type Type { get; }
        public abstract int TypeIndex { get; }
        public abstract Object Object { get; }
    }

    internal class Record<T> : Record
    {
        public override Type Type { get; }
        public override int TypeIndex { get; }
        public override Object Object => Value;
        public T Value { get; set; }

        public Record(T value, int typeIndex)
        {
            Type = typeof(T);
            TypeIndex = typeIndex;
            Value = value;
        }
    }
}