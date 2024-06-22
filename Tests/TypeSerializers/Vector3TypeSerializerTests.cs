using NUnit.Framework;
using UnityEngine;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector3TypeSerializerTests : BaseTypeSerializerTests<Vector3, Vector3Serializer>
    {
        private static Vector3[] Inputs => new[]
        {
            Vector3.left,
            Vector3.right,
            Vector3.down,
            Vector3.up,
            Vector3.forward,
            Vector3.back,
            Vector3.zero,
            Vector3.one,
            Vector3.positiveInfinity, // positive infinity
            Vector3.negativeInfinity, // negative infinity
            new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), // Max values
            new Vector3(float.MinValue, float.MinValue, float.MinValue), // Min values
            new Vector3(1.5f, -1.5f, 2.5f), // fractional values
            new Vector3(-2.3f, 4.5f, -3.3f), // mixed sign values
            new Vector3(123.456f, 789.123f, -456.789f), // large fractional values
            new Vector3(-0.1f, 0.1f, -0.1f), // small fractional values
            new Vector3(0f, -10f, 10f), // zero and positive/negative value
            new Vector3(-10f, 0f, -10f), // negative value and zero
            new Vector3(10f, 10f, 10f), // identical positive values
            new Vector3(-10f, -10f, -10f), // identical negative values
            new Vector3(0.001f, 0.001f, 0.001f), // very small positive values
            new Vector3(-0.001f, -0.001f, -0.001f), // very small negative values
            new Vector3(float.Epsilon, float.Epsilon, float.Epsilon), // smallest positive float
            new Vector3(float.PositiveInfinity, float.NegativeInfinity, 0), // mixed infinity and zero
            new Vector3(-0.5f, 0.5f, -0.5f), // small mixed sign values
            new Vector3(3.1415f, 2.7182f, 1.618f), // mathematical constants
        };

        public Vector3TypeSerializerTests(Vector3 value) : base(value)
        {
        }
    }
}