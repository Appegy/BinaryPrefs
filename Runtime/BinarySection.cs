using System.IO;

namespace Appegy.Storage
{
    internal abstract class BinarySection
    {
        public int Count { get; set; }
        public TypeSerializer Serializer { get; }

        protected BinarySection(TypeSerializer serializer)
        {
            Serializer = serializer;
        }

        public abstract string TypeName { get; }
        public abstract Record ReadFrom(BinaryReader binaryReader, int typeIndex);
        public abstract void WriteTo(BinaryWriter binaryWriter, Record record);
    }

    internal class TypedBinarySection<T> : BinarySection
    {
        private readonly TypeSerializer<T> _serializer;

        public new TypeSerializer<T> Serializer => _serializer;
        public override string TypeName => Serializer.TypeName;

        public TypedBinarySection(TypeSerializer<T> serializer)
            : base(serializer)
        {
            _serializer = serializer;
        }

        public override Record ReadFrom(BinaryReader binaryReader, int typeIndex)
        {
            return new Record<T>(_serializer.ReadFrom(binaryReader), typeIndex);
        }

        public override void WriteTo(BinaryWriter binaryWriter, Record record)
        {
            _serializer.WriteTo(binaryWriter, ((Record<T>)record).Value);
        }
    }
}