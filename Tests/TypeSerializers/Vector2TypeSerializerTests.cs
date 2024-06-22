using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector2TypeSerializerTests : BaseTypeSerializerTests<Vector2, Vector2Serializer>
    {
        private static Vector2[] Inputs => new[]
        {
            Vector2.left,
            Vector2.right,
            Vector2.down,
            Vector2.up,
            Vector2.zero,
            Vector2.one,
            Vector2.positiveInfinity, // positive infinity
            Vector2.negativeInfinity, // negative infinity
            new Vector2(float.MaxValue, float.MaxValue), // Max values
            new Vector2(float.MinValue, float.MinValue), // Min values
            new Vector2(1.5f, -1.5f), // fractional values
            new Vector2(-2.3f, 4.5f), // mixed sign values
            new Vector2(123.456f, 789.123f), // large fractional values
            new Vector2(-0.1f, 0.1f), // small fractional values
            new Vector2(0f, -10f), // zero and negative value
            new Vector2(-10f, 0f), // negative value and zero
            new Vector2(10f, 10f), // identical positive values
            new Vector2(-10f, -10f), // identical negative values
            new Vector2(0.001f, 0.001f), // very small positive values
            new Vector2(-0.001f, -0.001f), // very small negative values
            new Vector2(float.Epsilon, float.Epsilon) // smallest positive float
        };

        public Vector2TypeSerializerTests(Vector2 defaultValue) : base(defaultValue)
        {
        }
    }
}