using NUnit.Framework;
using UnityEngine;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector4TypeSerializerTests : BaseTypeSerializerTests<Vector4, Vector4Serializer>
    {
        private static Vector4[] Inputs => new[]
        {
            Vector4.zero, // all zeros
            Vector4.one, // all ones
            Vector4.positiveInfinity, // positive infinity
            Vector4.negativeInfinity, // negative infinity
            new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue), // Max values
            new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue), // Min values
            new Vector4(1.5f, -1.5f, 2.5f, -2.5f), // fractional values
            new Vector4(-2.3f, 4.5f, -3.3f, 6.7f), // mixed sign values
            new Vector4(123.456f, 789.123f, -456.789f, 321.654f), // large fractional values
            new Vector4(-0.1f, 0.1f, -0.1f, 0.1f), // small fractional values
            new Vector4(0f, -10f, 10f, -20f), // zero and positive/negative value
            new Vector4(-10f, 0f, -10f, 0f), // negative value and zero
            new Vector4(10f, 10f, 10f, 10f), // identical positive values
            new Vector4(-10f, -10f, -10f, -10f), // identical negative values
            new Vector4(0.001f, 0.001f, 0.001f, 0.001f), // very small positive values
            new Vector4(-0.001f, -0.001f, -0.001f, -0.001f), // very small negative values
            new Vector4(float.Epsilon, float.Epsilon, float.Epsilon, float.Epsilon), // smallest positive float
            new Vector4(float.PositiveInfinity, float.NegativeInfinity, 0, -0), // mixed infinity and zero
            new Vector4(-0.5f, 0.5f, -0.5f, 0.5f), // small mixed sign values
            new Vector4(3.1415f, 2.7182f, 1.618f, 0.5772f), // mathematical constants
            new Vector4(0f, 1f, 0f, 1f), // alternating zero and one
            new Vector4(-1f, -2f, -3f, -4f), // sequential negative values
            new Vector4(1f, 2f, 3f, 4f), // sequential positive values
        };

        public Vector4TypeSerializerTests(Vector4 value) : base(value)
        {
        }
    }
}