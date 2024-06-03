using System.IO;
using UnityEngine;

namespace Appegy.BinaryStorage
{
    internal class Vector2Serializer : EquatableTypeSerializer<Vector2>
    {
        public static Vector2Serializer Shared { get; } = new();
        public override string TypeName => "vector2f";
        public override int SizeOf(Vector2 _) => sizeof(float) * 2;

        public override void WriteTo(BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public override Vector2 ReadFrom(BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            return new Vector2(x, y);
        }
    }

    internal class Vector3Serializer : EquatableTypeSerializer<Vector3>
    {
        public static Vector3Serializer Shared { get; } = new();
        public override string TypeName => "vector3f";
        public override int SizeOf(Vector3 _) => sizeof(float) * 3;

        public override void WriteTo(BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public override Vector3 ReadFrom(BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
    }

    internal class Vector4Serializer : EquatableTypeSerializer<Vector4>
    {
        public static Vector4Serializer Shared { get; } = new();
        public override string TypeName => "vector4f";
        public override int SizeOf(Vector4 _) => sizeof(float) * 4;

        public override void WriteTo(BinaryWriter writer, Vector4 value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public override Vector4 ReadFrom(BinaryReader reader)
        {
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();
            return new Vector4(x, y, z, w);
        }
    }

    internal class Vector2IntSerializer : EquatableTypeSerializer<Vector2Int>
    {
        public static Vector2IntSerializer Shared { get; } = new();
        public override string TypeName => "vector2i";
        public override int SizeOf(Vector2Int _) => sizeof(int) * 2;

        public override void WriteTo(BinaryWriter writer, Vector2Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
        }

        public override Vector2Int ReadFrom(BinaryReader reader)
        {
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            return new Vector2Int(x, y);
        }
    }

    internal class Vector3IntSerializer : EquatableTypeSerializer<Vector3Int>
    {
        public static Vector3IntSerializer Shared { get; } = new();
        public override string TypeName => "vector3i";
        public override int SizeOf(Vector3Int _) => sizeof(int) * 3;

        public override void WriteTo(BinaryWriter writer, Vector3Int value)
        {
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public override Vector3Int ReadFrom(BinaryReader reader)
        {
            var x = reader.ReadInt32();
            var y = reader.ReadInt32();
            var z = reader.ReadInt32();
            return new Vector3Int(x, y, z);
        }
    }
}