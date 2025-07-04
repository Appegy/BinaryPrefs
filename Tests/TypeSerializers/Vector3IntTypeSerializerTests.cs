using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector3IntTypeSerializerTests : BaseTypeSerializerTests<Vector3Int, Vector3IntSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { Vector3Int.left, "left direction" },
            new object[] { Vector3Int.right, "right direction" },
            new object[] { Vector3Int.down, "down direction" },
            new object[] { Vector3Int.up, "up direction" },
            new object[] { Vector3Int.forward, "forward direction" },
            new object[] { Vector3Int.back, "back direction" },
            new object[] { Vector3Int.zero, "zero vector" },
            new object[] { Vector3Int.one, "one vector" },
            new object[] { new Vector3Int(int.MaxValue, int.MaxValue, int.MaxValue), "Max values" },
            new object[] { new Vector3Int(int.MinValue, int.MinValue, int.MinValue), "Min values" },
            new object[] { new Vector3Int(1, -1, 1), "mixed sign values" },
            new object[] { new Vector3Int(-2, 4, -2), "mixed sign values" },
            new object[] { new Vector3Int(123, 789, -456), "large values" },
            new object[] { new Vector3Int(-10, 0, 10), "negative, zero, positive values" },
            new object[] { new Vector3Int(0, -10, 10), "zero, negative, positive values" },
            new object[] { new Vector3Int(10, 10, 10), "identical positive values" },
            new object[] { new Vector3Int(-10, -10, -10), "identical negative values" },
            new object[] { new Vector3Int(1, 1, 1), "small positive values" },
            new object[] { new Vector3Int(-1, -1, -1), "small negative values" },
            new object[] { new Vector3Int(0, 1, 0), "zero and positive value" },
            new object[] { new Vector3Int(1, 0, 1), "positive value and zero" },
            new object[] { new Vector3Int(-1, 0, -1), "negative value and zero" },
            new object[] { new Vector3Int(0, -1, 0), "zero and negative value" },
            new object[] { new Vector3Int(100, -100, 100), "mixed large values" },
            new object[] { new Vector3Int(-100, 100, -100), "mixed large values" },
            new object[] { new Vector3Int(256, -256, 256), "powers of 2 values" },
            new object[] { new Vector3Int(-256, 256, -256), "powers of 2 values" },
            new object[] { new Vector3Int(32767, -32768, 32767), "max and min short values" },
            new object[] { new Vector3Int(-32768, 32767, -32768), "min and max short values" }
        };

        public Vector3IntTypeSerializerTests(Vector3Int value, string _) : base(value)
        {
        }
    }
}