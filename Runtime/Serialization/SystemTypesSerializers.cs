using System;
using System.Buffers;
using System.IO;
using System.Text;

namespace Appegy.BinaryStorage
{
    internal class BooleanSerializer : EquatableTypeSerializer<bool>
    {
        public static BooleanSerializer Shared { get; } = new();
        public override string TypeName => "bool";
        public override int SizeOf(bool _) => sizeof(bool);
        public override void WriteTo(BinaryWriter writer, bool value) => writer.Write(value);
        public override bool ReadFrom(BinaryReader reader) => reader.ReadBoolean();
    }

    internal class CharSerializer : EquatableTypeSerializer<char>
    {
        public static CharSerializer Shared { get; } = new();
        public override string TypeName => "char";
        public override int SizeOf(char value) => sizeof(char);

        public override void WriteTo(BinaryWriter writer, char value)
        {
            var size = sizeof(char);
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            BitConverter.TryWriteBytes(buffer, value);
            writer.Write(buffer, 0, size);
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public override char ReadFrom(BinaryReader reader)
        {
            var size = sizeof(char);
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            var read = reader.BaseStream.Read(buffer, 0, size);
            if (read != size)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                return default;
            }
            var value = BitConverter.ToChar(buffer);
            ArrayPool<byte>.Shared.Return(buffer);
            return value;
        }
    }

    internal class ByteSerializer : EquatableTypeSerializer<byte>
    {
        public static ByteSerializer Shared { get; } = new();
        public override string TypeName => "byte";
        public override int SizeOf(byte _) => sizeof(byte);
        public override void WriteTo(BinaryWriter writer, byte value) => writer.Write(value);
        public override byte ReadFrom(BinaryReader reader) => reader.ReadByte();
    }

    internal class SByteSerializer : EquatableTypeSerializer<sbyte>
    {
        public static SByteSerializer Shared { get; } = new();
        public override string TypeName => "sbyte";
        public override int SizeOf(sbyte _) => sizeof(sbyte);
        public override void WriteTo(BinaryWriter writer, sbyte value) => writer.Write(value);
        public override sbyte ReadFrom(BinaryReader reader) => reader.ReadSByte();
    }

    internal class Int16Serializer : EquatableTypeSerializer<short>
    {
        public static Int16Serializer Shared { get; } = new();
        public override string TypeName => "short";
        public override int SizeOf(short _) => sizeof(short);
        public override void WriteTo(BinaryWriter writer, short value) => writer.Write(value);
        public override short ReadFrom(BinaryReader reader) => reader.ReadInt16();
    }

    internal class UInt16Serializer : EquatableTypeSerializer<ushort>
    {
        public static UInt16Serializer Shared { get; } = new();
        public override string TypeName => "ushort";
        public override int SizeOf(ushort _) => sizeof(short);
        public override void WriteTo(BinaryWriter writer, ushort value) => writer.Write(value);
        public override ushort ReadFrom(BinaryReader reader) => reader.ReadUInt16();
    }

    internal class Int32Serializer : EquatableTypeSerializer<int>
    {
        public static Int32Serializer Shared { get; } = new();
        public override string TypeName => "int";
        public override int SizeOf(int _) => sizeof(int);
        public override void WriteTo(BinaryWriter writer, int value) => writer.Write(value);
        public override int ReadFrom(BinaryReader reader) => reader.ReadInt32();
    }

    internal class UInt32Serializer : EquatableTypeSerializer<uint>
    {
        public static UInt32Serializer Shared { get; } = new();
        public override string TypeName => "uint";
        public override int SizeOf(uint _) => sizeof(uint);
        public override void WriteTo(BinaryWriter writer, uint value) => writer.Write(value);
        public override uint ReadFrom(BinaryReader reader) => reader.ReadUInt32();
    }

    internal class Int64Serializer : EquatableTypeSerializer<long>
    {
        public static Int64Serializer Shared { get; } = new();
        public override string TypeName => "long";
        public override int SizeOf(long _) => sizeof(long);
        public override void WriteTo(BinaryWriter writer, long value) => writer.Write(value);
        public override long ReadFrom(BinaryReader reader) => reader.ReadInt64();
    }

    internal class UInt64Serializer : EquatableTypeSerializer<ulong>
    {
        public static UInt64Serializer Shared { get; } = new();
        public override string TypeName => "ulong";
        public override int SizeOf(ulong _) => sizeof(ulong);
        public override void WriteTo(BinaryWriter writer, ulong value) => writer.Write(value);
        public override ulong ReadFrom(BinaryReader reader) => reader.ReadUInt64();
    }

    internal class SingleSerializer : EquatableTypeSerializer<float>
    {
        public static SingleSerializer Shared { get; } = new();
        public override string TypeName => "float";
        public override int SizeOf(float _) => sizeof(float);
        public override void WriteTo(BinaryWriter writer, float value) => writer.Write(value);
        public override float ReadFrom(BinaryReader reader) => reader.ReadSingle();
    }

    internal class DoubleSerializer : EquatableTypeSerializer<double>
    {
        public static DoubleSerializer Shared { get; } = new();
        public override string TypeName => "double";
        public override int SizeOf(double _) => sizeof(double);
        public override void WriteTo(BinaryWriter writer, double value) => writer.Write(value);
        public override double ReadFrom(BinaryReader reader) => reader.ReadDouble();
    }

    internal class DecimalSerializer : EquatableTypeSerializer<decimal>
    {
        public static DecimalSerializer Shared { get; } = new();
        public override string TypeName => "decimal";
        public override int SizeOf(decimal _) => sizeof(decimal);
        public override void WriteTo(BinaryWriter writer, decimal value) => writer.Write(value);
        public override decimal ReadFrom(BinaryReader reader) => reader.ReadDecimal();
    }

    internal class StringSerializer : EquatableTypeSerializerRef<string>
    {
        public static StringSerializer Shared { get; } = new();
        public static readonly Encoding Encoding = Encoding.UTF8;

        public override string TypeName => "string";
        public override int SizeOf(string value) => sizeof(int) + (value != null ? Encoding.GetByteCount(value) : 0);

        public override void WriteTo(BinaryWriter writer, string value)
        {
            if (value == null)
            {
                writer.Write(-1);
            }
            else if (value.Length == 0)
            {
                writer.Write(0);
            }
            else
            {
                var size = SizeOf(value);
                var buffer = ArrayPool<byte>.Shared.Rent(size);
                var bufferSize = Encoding.GetBytes(value, buffer);
                writer.Write(bufferSize);
                writer.Write(buffer, 0, bufferSize);
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public override string ReadFrom(BinaryReader reader)
        {
            var size = reader.ReadInt32();
            if (size == -1)
            {
                return null;
            }
            if (size == 0)
            {
                return string.Empty;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            var read = reader.BaseStream.Read(buffer, 0, size);
            if (read != size)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                return string.Empty;
            }
            var value = Encoding.GetString(buffer, 0, size);
            ArrayPool<byte>.Shared.Return(buffer);
            return value;
        }
    }

    internal class DateTimeSerializer : EquatableTypeSerializer<DateTime>
    {
        public static DateTimeSerializer Shared { get; } = new();

        public override string TypeName => "DateTime";
        public override int SizeOf(DateTime _) => sizeof(long);

        public override void WriteTo(BinaryWriter writer, DateTime value) => writer.Write(value.ToBinary());

        public override DateTime ReadFrom(BinaryReader reader) => DateTime.FromBinary(reader.ReadInt64());
    }

    internal class TimeSpanSerializer : EquatableTypeSerializer<TimeSpan>
    {
        public static TimeSpanSerializer Shared { get; } = new();

        public override string TypeName => "TimeSpan";
        public override int SizeOf(TimeSpan _) => sizeof(long);

        public override void WriteTo(BinaryWriter writer, TimeSpan value) => writer.Write(value.Ticks);

        public override TimeSpan ReadFrom(BinaryReader reader) => TimeSpan.FromTicks(reader.ReadInt64());
    }
}