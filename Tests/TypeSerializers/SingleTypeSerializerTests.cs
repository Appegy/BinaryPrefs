using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class SingleTypeSerializerTests : BaseTypeSerializerTests<float, SingleSerializer>
    {
        private static float[] Inputs => new[]
        {
            float.MinValue, // -3.40282347E+38
            float.MaxValue, // 3.40282347E+38
            float.Epsilon, // 1.401298E-45
            float.NegativeInfinity, // Negative infinity
            float.PositiveInfinity, // Positive infinity
            0f, // zero
            1f, // smallest positive float
            -1f, // smallest negative float
            3.14159f, // pi
            -3.14159f // negative pi
        };

        public SingleTypeSerializerTests(float value) : base(value)
        {
        }
    }
}