using System;

namespace Appegy.Storage
{
    internal abstract class Record
    {
        public abstract Type Type { get; }
        public abstract int TypeIndex { get; }
        public abstract Object Object { get; }

        public abstract IReactiveCollection AsReactiveCollection();
    }

    internal class Record<T> : Record
    {
        private static readonly bool ValueCanBeReactiveCollection = typeof(IReactiveCollection).IsAssignableFrom(typeof(T));

        public override Type Type { get; }
        public override int TypeIndex { get; }
        public override Object Object => Value;
        public T Value { get; set; }

        public override IReactiveCollection AsReactiveCollection()
        {
            return ValueCanBeReactiveCollection ? (IReactiveCollection)(object)Value : null;
        }

        public Record(T value, int typeIndex)
        {
            Type = typeof(T);
            TypeIndex = typeIndex;
            Value = value;
        }
    }
}
