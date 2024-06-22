using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class QuaternionTypeSerializerTests : BaseTypeSerializerTests<Quaternion, QuaternionSerializer>
    {
        private static Quaternion[] Inputs => new[]
        {
            Quaternion.identity, // identity quaternion
            new Quaternion(0, 0, 0, 0), // zero quaternion
            new Quaternion(1, 0, 0, 0), // unit x quaternion
            new Quaternion(0, 1, 0, 0), // unit y quaternion
            new Quaternion(0, 0, 1, 0), // unit z quaternion
            new Quaternion(0, 0, 0, 1), // unit w quaternion
            new Quaternion(1, 1, 1, 1), // all ones
            new Quaternion(-1, -1, -1, -1), // all negatives
            new Quaternion(float.MaxValue, 0, 0, 0), // max value x component
            new Quaternion(0, float.MaxValue, 0, 0), // max value y component
            new Quaternion(0, 0, float.MaxValue, 0), // max value z component
            new Quaternion(0, 0, 0, float.MaxValue), // max value w component
            new Quaternion(float.MinValue, 0, 0, 0), // min value x component
            new Quaternion(0, float.MinValue, 0, 0), // min value y component
            new Quaternion(0, 0, float.MinValue, 0), // min value z component
            new Quaternion(0, 0, 0, float.MinValue), // min value w component
            new Quaternion(0.707f, 0.707f, 0, 0), // 90 degree rotation around x-axis
            new Quaternion(0, 0.707f, 0.707f, 0), // 90 degree rotation around y-axis
            new Quaternion(0, 0, 0.707f, 0.707f), // 90 degree rotation around z-axis
            new Quaternion(0.5f, 0.5f, 0.5f, 0.5f), // normalized quaternion with equal components
            new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), // negative normalized quaternion with equal components
            new Quaternion(0.1f, 0.2f, 0.3f, 0.4f), // non-unit quaternion
            new Quaternion(1e-10f, 1e-10f, 1e-10f, 1e-10f), // very small components
            new Quaternion(1e10f, 1e10f, 1e10f, 1e10f), // very large components
            new Quaternion(float.PositiveInfinity, 0, 0, 0), // positive infinity x component
            new Quaternion(0, float.PositiveInfinity, 0, 0), // positive infinity y component
            new Quaternion(0, 0, float.PositiveInfinity, 0), // positive infinity z component
            new Quaternion(0, 0, 0, float.PositiveInfinity), // positive infinity w component
            new Quaternion(float.NegativeInfinity, 0, 0, 0), // negative infinity x component
            new Quaternion(0, float.NegativeInfinity, 0, 0), // negative infinity y component
            new Quaternion(0, 0, float.NegativeInfinity, 0), // negative infinity z component
            new Quaternion(0, 0, 0, float.NegativeInfinity) // negative infinity w component
        };

        public QuaternionTypeSerializerTests(Quaternion value) : base(value)
        {
        }
    }
}