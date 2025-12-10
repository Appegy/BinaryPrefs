using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector3TypeSerializerTests : BaseTypeSerializerTests<Vector3, Vector3Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { Vector3.left, "left" },
            new object[] { Vector3.right, "right" },
            new object[] { Vector3.down, "down" },
            new object[] { Vector3.up, "up" },
            new object[] { Vector3.forward, "forward" },
            new object[] { Vector3.back, "back" },
            new object[] { Vector3.zero, "zero" },
            new object[] { Vector3.one, "one" },
            new object[] { Vector3.positiveInfinity, "positive infinity" },
            new object[] { Vector3.negativeInfinity, "negative infinity" },
            new object[] { new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), "Max values" },
            new object[] { new Vector3(float.MinValue, float.MinValue, float.MinValue), "Min values" },
            new object[] { new Vector3(1.5f, -1.5f, 2.5f), "fractional values" },
            new object[] { new Vector3(-2.3f, 4.5f, -3.3f), "mixed sign values" },
            new object[] { new Vector3(123.456f, 789.123f, -456.789f), "large fractional values" },
            new object[] { new Vector3(-0.1f, 0.1f, -0.1f), "small fractional values" },
            new object[] { new Vector3(0f, -10f, 10f), "zero and positive/negative value" },
            new object[] { new Vector3(-10f, 0f, -10f), "negative value and zero" },
            new object[] { new Vector3(10f, 10f, 10f), "identical positive values" },
            new object[] { new Vector3(-10f, -10f, -10f), "identical negative values" },
            new object[] { new Vector3(0.001f, 0.001f, 0.001f), "very small positive values" },
            new object[] { new Vector3(-0.001f, -0.001f, -0.001f), "very small negative values" },
            new object[] { new Vector3(float.Epsilon, float.Epsilon, float.Epsilon), "smallest positive float" },
            new object[] { new Vector3(float.PositiveInfinity, float.NegativeInfinity, 0), "mixed infinity and zero" },
            new object[] { new Vector3(-0.5f, 0.5f, -0.5f), "small mixed sign values" },
            new object[] { new Vector3(3.1415f, 2.7182f, 1.618f), "mathematical constants" }
        };

        public Vector3TypeSerializerTests(Vector3 value, string _) : base(value)
        {
        }
    }
}