using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Vector2TypeSerializerTests : BaseTypeSerializerTests<Vector2, Vector2Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { Vector2.left, "left" },
            new object[] { Vector2.right, "right" },
            new object[] { Vector2.down, "down" },
            new object[] { Vector2.up, "up" },
            new object[] { Vector2.zero, "zero" },
            new object[] { Vector2.one, "one" },
            new object[] { Vector2.positiveInfinity, "positive infinity" },
            new object[] { Vector2.negativeInfinity, "negative infinity" },
            new object[] { new Vector2(float.MaxValue, float.MaxValue), "Max values" },
            new object[] { new Vector2(float.MinValue, float.MinValue), "Min values" },
            new object[] { new Vector2(1.5f, -1.5f), "fractional values" },
            new object[] { new Vector2(-2.3f, 4.5f), "mixed sign values" },
            new object[] { new Vector2(123.456f, 789.123f), "large fractional values" },
            new object[] { new Vector2(-0.1f, 0.1f), "small fractional values" },
            new object[] { new Vector2(0f, -10f), "zero and negative value" },
            new object[] { new Vector2(-10f, 0f), "negative value and zero" },
            new object[] { new Vector2(10f, 10f), "identical positive values" },
            new object[] { new Vector2(-10f, -10f), "identical negative values" },
            new object[] { new Vector2(0.001f, 0.001f), "very small positive values" },
            new object[] { new Vector2(-0.001f, -0.001f), "very small negative values" },
            new object[] { new Vector2(float.Epsilon, float.Epsilon), "smallest positive float" }
        };

        public Vector2TypeSerializerTests(Vector2 defaultValue, string _) : base(defaultValue)
        {
        }
    }
}