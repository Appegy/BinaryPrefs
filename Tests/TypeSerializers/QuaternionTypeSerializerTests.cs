using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class QuaternionTypeSerializerTests : BaseTypeSerializerTests<Quaternion, QuaternionSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { Quaternion.identity, "identity" },
            new object[] { new Quaternion(0, 0, 0, 0), "zero" },
            new object[] { new Quaternion(1, 0, 0, 0), "unit_x" },
            new object[] { new Quaternion(0, 1, 0, 0), "unit_y" },
            new object[] { new Quaternion(0, 0, 1, 0), "unit_z" },
            new object[] { new Quaternion(0, 0, 0, 1), "unit_w" },
            new object[] { new Quaternion(1, 1, 1, 1), "all_ones" },
            new object[] { new Quaternion(-1, -1, -1, -1), "all_negatives" },
            new object[] { new Quaternion(float.MaxValue, 0, 0, 0), "max_x" },
            new object[] { new Quaternion(0, float.MaxValue, 0, 0), "max_y" },
            new object[] { new Quaternion(0, 0, float.MaxValue, 0), "max_z" },
            new object[] { new Quaternion(0, 0, 0, float.MaxValue), "max_w" },
            new object[] { new Quaternion(float.MinValue, 0, 0, 0), "min_x" },
            new object[] { new Quaternion(0, float.MinValue, 0, 0), "min_y" },
            new object[] { new Quaternion(0, 0, float.MinValue, 0), "min_z" },
            new object[] { new Quaternion(0, 0, 0, float.MinValue), "min_w" },
            new object[] { new Quaternion(0.707f, 0.707f, 0, 0), "rot_x_90" },
            new object[] { new Quaternion(0, 0.707f, 0.707f, 0), "rot_y_90" },
            new object[] { new Quaternion(0, 0, 0.707f, 0.707f), "rot_z_90" },
            new object[] { new Quaternion(0.5f, 0.5f, 0.5f, 0.5f), "all_half" },
            new object[] { new Quaternion(-0.5f, -0.5f, -0.5f, -0.5f), "all_minus_half" },
            new object[] { new Quaternion(0.1f, 0.2f, 0.3f, 0.4f), "small_components" },
            new object[] { new Quaternion(1e-10f, 1e-10f, 1e-10f, 1e-10f), "very_small" },
            new object[] { new Quaternion(1e10f, 1e10f, 1e10f, 1e10f), "very_large" },
            new object[] { new Quaternion(float.PositiveInfinity, 0, 0, 0), "pos_inf_x" },
            new object[] { new Quaternion(0, float.PositiveInfinity, 0, 0), "pos_inf_y" },
            new object[] { new Quaternion(0, 0, float.PositiveInfinity, 0), "pos_inf_z" },
            new object[] { new Quaternion(0, 0, 0, float.PositiveInfinity), "pos_inf_w" },
            new object[] { new Quaternion(float.NegativeInfinity, 0, 0, 0), "neg_inf_x" },
            new object[] { new Quaternion(0, float.NegativeInfinity, 0, 0), "neg_inf_y" },
            new object[] { new Quaternion(0, 0, float.NegativeInfinity, 0), "neg_inf_z" },
            new object[] { new Quaternion(0, 0, 0, float.NegativeInfinity), "neg_inf_w" }
        };

        public QuaternionTypeSerializerTests(Quaternion value, string _) : base(value)
        {
        }
    }
}