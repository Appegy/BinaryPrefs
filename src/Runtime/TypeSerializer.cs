using System;
using System.Collections.Generic;
using System.IO;

namespace Appegy.Storage
{
    public abstract class TypeSerializer
    {
        public abstract string TypeName { get; }
        public virtual IReadOnlyList<string> FallbackNames => Array.Empty<string>();
    }

    public abstract class TypeSerializer<T> : TypeSerializer
    {
        public override string TypeName { get; } = typeof(T).FullName;
        public abstract bool Equals(T value1, T value2);
        public abstract void WriteTo(BinaryWriter writer, T value);
        public abstract T ReadFrom(BinaryReader reader);
    }

    public abstract class EquatableTypeSerializer<T> : TypeSerializer<T>
        where T : struct, IEquatable<T>
    {
        public sealed override bool Equals(T value1, T value2)
        {
            return value1.Equals(value2);
        }
    }

    public abstract class EquatableTypeSerializerRef<T> : TypeSerializer<T>
        where T : class, IEquatable<T>
    {
        public sealed override bool Equals(T value1, T value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            if (value1 == null || value2 == null)
            {
                return false;
            }
            return value1.Equals(value2);
        }
    }
}