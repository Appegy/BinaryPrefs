using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector2IntTypeSerializerTests : BaseTypeSerializerTests<Vector2Int, Vector2IntSerializer>
    {
        private static Vector2Int[] Inputs => new[]
        {
            Vector2Int.left, // left direction
            Vector2Int.right, // right direction
            Vector2Int.down, // down direction
            Vector2Int.up, // up direction
            Vector2Int.zero, // zero vector
            Vector2Int.one, // one vector
            new Vector2Int(int.MaxValue, int.MaxValue), // Max values
            new Vector2Int(int.MinValue, int.MinValue), // Min values
            new Vector2Int(1, -1), // mixed sign values
            new Vector2Int(-2, 4), // mixed sign values
            new Vector2Int(123, 789), // large values
            new Vector2Int(-10, 0), // negative value and zero
            new Vector2Int(0, -10), // zero and negative value
            new Vector2Int(10, 10), // identical positive values
            new Vector2Int(-10, -10), // identical negative values
            new Vector2Int(1, 1), // small positive values
            new Vector2Int(-1, -1), // small negative values
            new Vector2Int(0, 1), // zero and positive value
            new Vector2Int(1, 0), // positive value and zero
            new Vector2Int(-1, 0), // negative value and zero
            new Vector2Int(0, -1), // zero and negative value
            new Vector2Int(100, -100), // mixed large values
            new Vector2Int(-100, 100), // mixed large values
            new Vector2Int(256, -256), // powers of 2 values
            new Vector2Int(-256, 256), // powers of 2 values
            new Vector2Int(32767, -32768), // max and min short values
            new Vector2Int(-32768, 32767) // min and max short values
        };

        public Vector2IntTypeSerializerTests(Vector2Int value) : base(value)
        {
        }
    }
}