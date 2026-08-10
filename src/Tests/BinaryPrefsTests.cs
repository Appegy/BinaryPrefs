using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    public class BinaryPrefsTestsBase
    {
        protected static readonly string PrefsPath = Path.Combine(Application.temporaryCachePath, "test_prefs.bin");

        [SetUp, TearDown]
        public void CleanPrefsBetweenTests()
        {
            BinaryPrefs.Reset();
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            if (File.Exists(PrefsPath))
            {
                File.Delete(PrefsPath);
            }
            BinaryPrefs.OverrideStorageFilePath(PrefsPath);
        }
    }

    [TestFixture]
    public class BinaryPrefsPrimitiveTests : BinaryPrefsTestsBase
    {
        [Test]
        public void SetInt_ShouldStoreValueInBinaryStorage()
        {
            BinaryPrefs.SetInt("key", 42);

            BinaryPrefs.GetInt("key").Should().Be(42);
            BinaryPrefs.TypeOf("key").Should().Be(typeof(int));
        }

        [Test]
        public void GetInt_ShouldReturnDefaultValueIfKeyNotFound()
        {
            BinaryPrefs.GetInt("unknownKey", 10).Should().Be(10);
        }

        [Test]
        public void SetInt_ShouldOverrideExistingValue()
        {
            BinaryPrefs.SetInt("key", 42);
            BinaryPrefs.SetInt("key", 84);

            BinaryPrefs.GetInt("key").Should().Be(84);
        }

        [Test]
        public void SetFloat_ShouldStoreValueInBinaryStorage()
        {
            BinaryPrefs.SetFloat("key", 42.5f);

            BinaryPrefs.GetFloat("key").Should().Be(42.5f);
            BinaryPrefs.TypeOf("key").Should().Be(typeof(float));
        }

        [Test]
        public void GetFloat_ShouldReturnDefaultValueIfKeyNotFound()
        {
            BinaryPrefs.GetFloat("unknownKey", 10f).Should().Be(10f);
        }

        [Test]
        public void SetString_ShouldStoreValueInBinaryStorage()
        {
            BinaryPrefs.SetString("key", "value");

            BinaryPrefs.GetString("key").Should().Be("value");
            BinaryPrefs.TypeOf("key").Should().Be(typeof(string));
        }

        [Test]
        public void GetString_ShouldReturnDefaultValueIfKeyNotFound()
        {
            BinaryPrefs.GetString("unknownKey", "fallback").Should().Be("fallback");
        }

        [Test]
        public void SetBool_ShouldStoreValueInBinaryStorage()
        {
            BinaryPrefs.SetBool("key", true);

            BinaryPrefs.GetBool("key").Should().BeTrue();
            BinaryPrefs.TypeOf("key").Should().Be(typeof(bool));
        }

        [Test]
        public void GetBool_ShouldReturnDefaultValueIfKeyNotFound()
        {
            BinaryPrefs.GetBool("unknownKey", true).Should().BeTrue();
        }
    }

    [TestFixture]
    public class BinaryPrefsTypeMismatchTests : BinaryPrefsTestsBase
    {
        [Test]
        public void GetString_ShouldNotThrowWhenKeyStoredAsInt()
        {
            BinaryPrefs.SetInt("key", 42);

            FluentActions.Invoking(() => BinaryPrefs.GetString("key")).Should().NotThrow();
        }

        [Test]
        public void GetString_ShouldReturnDefaultWhenKeyStoredAsInt()
        {
            BinaryPrefs.SetInt("key", 42);

            BinaryPrefs.GetString("key", "fallback").Should().Be("fallback");
        }

        [Test]
        public void GetInt_ShouldReturnDefaultWhenKeyStoredAsFloat()
        {
            BinaryPrefs.SetFloat("key", 42.5f);

            BinaryPrefs.GetInt("key", 7).Should().Be(7);
        }

        [Test]
        public void GetInt_ShouldReturnDefaultWhenKeyStoredAsBool()
        {
            BinaryPrefs.SetBool("key", true);

            BinaryPrefs.GetInt("key", 7).Should().Be(7);
        }

        [Test]
        public void Set_ShouldReplaceTypeOfExistingKey()
        {
            BinaryPrefs.SetInt("key", 42);
            BinaryPrefs.SetString("key", "value");

            BinaryPrefs.GetString("key").Should().Be("value");
            BinaryPrefs.TypeOf("key").Should().Be(typeof(string));
        }
    }

    [TestFixture]
    public class BinaryPrefsMigrationTests : BinaryPrefsTestsBase
    {
        [Test]
        public void GetInt_ShouldReturnValueFromPlayerPrefsWhenNotInBinaryStorage()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.GetInt("key").Should().Be(100);
        }

        [Test]
        public void GetInt_ShouldMoveValueIntoBinaryStorage()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.GetInt("key");

            BinaryPrefs.TypeOf("key").Should().Be(typeof(int));
        }

        [Test]
        public void GetInt_ShouldRemoveMigratedKeyFromPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.GetInt("key");

            PlayerPrefs.HasKey("key").Should().BeFalse();
        }

        [Test]
        public void GetFloat_ShouldMigrateValueFromPlayerPrefs()
        {
            PlayerPrefs.SetFloat("key", 100.5f);

            BinaryPrefs.GetFloat("key").Should().Be(100.5f);
            PlayerPrefs.HasKey("key").Should().BeFalse();
            BinaryPrefs.TypeOf("key").Should().Be(typeof(float));
        }

        [Test]
        public void GetString_ShouldMigrateValueFromPlayerPrefs()
        {
            PlayerPrefs.SetString("key", "legacy");

            BinaryPrefs.GetString("key").Should().Be("legacy");
            PlayerPrefs.HasKey("key").Should().BeFalse();
            BinaryPrefs.TypeOf("key").Should().Be(typeof(string));
        }

        [Test]
        public void GetBool_ShouldMigrateIntValueFromPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 1);

            BinaryPrefs.GetBool("key").Should().BeTrue();
            PlayerPrefs.HasKey("key").Should().BeFalse();
            BinaryPrefs.TypeOf("key").Should().Be(typeof(bool));
        }

        [Test]
        public void GetBool_ShouldMigrateZeroIntAsFalse()
        {
            PlayerPrefs.SetInt("key", 0);

            BinaryPrefs.GetBool("key", true).Should().BeFalse();
        }

        [Test]
        public void GetBool_ShouldNotMigrateIntValueOutsideZeroAndOne()
        {
            PlayerPrefs.SetInt("key", 5);

            BinaryPrefs.GetBool("key", true).Should().BeTrue();
        }

        [Test]
        public void GetBool_ShouldNotDeleteIntValueOutsideZeroAndOne()
        {
            PlayerPrefs.SetInt("key", 5);

            BinaryPrefs.GetBool("key");

            BinaryPrefs.GetInt("key").Should().Be(5);
        }

        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        public void GetInt_ShouldMigrateValuesEqualToProbeSentinels(int value)
        {
            PlayerPrefs.SetInt("key", value);

            BinaryPrefs.GetInt("key").Should().Be(value);
        }

        [TestCase(float.MinValue)]
        [TestCase(float.MaxValue)]
        [TestCase(0f)]
        public void GetFloat_ShouldMigrateValuesEqualToProbeSentinels(float value)
        {
            PlayerPrefs.SetFloat("key", value);

            BinaryPrefs.GetFloat("key").Should().Be(value);
        }

        [Test]
        public void GetInt_ShouldNotMigrateKeyStoredAsStringInPlayerPrefs()
        {
            PlayerPrefs.SetString("key", "legacy");

            BinaryPrefs.GetInt("key", 7).Should().Be(7);
        }

        [Test]
        public void GetInt_ShouldNotDeleteKeyStoredAsStringInPlayerPrefs()
        {
            PlayerPrefs.SetString("key", "legacy");

            BinaryPrefs.GetInt("key");

            PlayerPrefs.GetString("key").Should().Be("legacy");
        }

        [Test]
        public void GetString_ShouldNotMigrateKeyStoredAsIntInPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.GetString("key", "fallback").Should().Be("fallback");
            PlayerPrefs.GetInt("key").Should().Be(100);
        }

        [Test]
        public void SetString_ShouldRemoveShadowedPlayerPrefsKey()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.SetString("key", "value");

            PlayerPrefs.HasKey("key").Should().BeFalse();
        }

        [Test]
        public void SetString_ShouldRemoveShadowedPlayerPrefsKeyAfterStorageReload()
        {
            PlayerPrefs.SetInt("key", 100);
            BinaryPrefs.SetString("other", "value");
            BinaryPrefs.OverrideStorageFilePath(PrefsPath);

            BinaryPrefs.SetString("key", "value");

            PlayerPrefs.HasKey("key").Should().BeFalse();
        }

        [Test]
        public void GetInt_ShouldNotResurrectPlayerPrefsValueAfterKeyWasOverwrittenWithAnotherType()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.SetString("key", "value");

            BinaryPrefs.GetInt("key", 7).Should().Be(7);
        }
    }

    [TestFixture]
    public class BinaryPrefsDeletionTests : BinaryPrefsTestsBase
    {
        [Test]
        public void HasKey_ShouldBeTrueForKeyOnlyInPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.HasKey("key").Should().BeTrue();
        }

        [Test]
        public void HasKey_ShouldBeFalseForUnknownKey()
        {
            BinaryPrefs.HasKey("unknownKey").Should().BeFalse();
        }

        [Test]
        public void DeleteKey_ShouldRemoveKeyFromBinaryStorage()
        {
            BinaryPrefs.SetInt("key", 42);

            BinaryPrefs.DeleteKey("key");

            BinaryPrefs.HasKey("key").Should().BeFalse();
        }

        [Test]
        public void DeleteKey_ShouldRemoveKeyFromPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.DeleteKey("key");

            PlayerPrefs.HasKey("key").Should().BeFalse();
        }

        [Test]
        public void DeleteKey_ShouldNotResurrectValueFromPlayerPrefs()
        {
            PlayerPrefs.SetInt("key", 100);

            BinaryPrefs.DeleteKey("key");

            BinaryPrefs.GetInt("key", 7).Should().Be(7);
        }

        [Test]
        public void DeleteAll_ShouldRemoveEverythingFromBothStorages()
        {
            BinaryPrefs.SetInt("stored", 42);
            PlayerPrefs.SetInt("legacy", 100);

            BinaryPrefs.DeleteAll();

            BinaryPrefs.HasKey("stored").Should().BeFalse();
            BinaryPrefs.HasKey("legacy").Should().BeFalse();
        }

        [Test]
        public void DeleteAll_ShouldNotResurrectValuesFromPlayerPrefs()
        {
            PlayerPrefs.SetInt("legacy", 100);

            BinaryPrefs.DeleteAll();

            BinaryPrefs.GetInt("legacy", 7).Should().Be(7);
        }
    }

    [TestFixture]
    public class BinaryPrefsPersistenceTests : BinaryPrefsTestsBase
    {
        [Test]
        public void Values_ShouldSurviveStorageReload()
        {
            BinaryPrefs.SetInt("int", 42);
            BinaryPrefs.SetString("string", "value");
            BinaryPrefs.SetBool("bool", true);

            ReopenStorage();

            BinaryPrefs.GetInt("int").Should().Be(42);
            BinaryPrefs.GetString("string").Should().Be("value");
            BinaryPrefs.GetBool("bool").Should().BeTrue();
        }

        [Test]
        public void MigratedValues_ShouldSurviveStorageReload()
        {
            PlayerPrefs.SetInt("key", 100);
            BinaryPrefs.GetInt("key");

            ReopenStorage();

            BinaryPrefs.GetInt("key").Should().Be(100);
        }

        [Test]
        public void DeletedKeys_ShouldNotComeBackAfterStorageReload()
        {
            BinaryPrefs.SetInt("key", 42);
            BinaryPrefs.DeleteKey("key");

            ReopenStorage();

            BinaryPrefs.HasKey("key").Should().BeFalse();
        }

        private static void ReopenStorage()
        {
            BinaryPrefs.Save();
            BinaryPrefs.OverrideStorageFilePath(PrefsPath);
        }
    }

    [TestFixture]
    public class BinaryPrefsExtendedTypesTests : BinaryPrefsTestsBase
    {
        [Test]
        public void SetLong_GetLong_ShouldRoundTrip()
        {
            BinaryPrefs.SetLong("key", long.MaxValue);

            BinaryPrefs.GetLong("key").Should().Be(long.MaxValue);
        }

        [Test]
        public void SetDouble_GetDouble_ShouldRoundTrip()
        {
            BinaryPrefs.SetDouble("key", 42.125d);

            BinaryPrefs.GetDouble("key").Should().Be(42.125d);
        }

        [Test]
        public void SetDateTime_GetDateTime_ShouldRoundTrip()
        {
            var value = new DateTime(2024, 5, 17, 13, 45, 30, DateTimeKind.Utc);

            BinaryPrefs.SetDateTime("key", value);

            BinaryPrefs.GetDateTime("key").Should().Be(value);
        }

        [Test]
        public void SetTimeSpan_GetTimeSpan_ShouldRoundTrip()
        {
            var value = TimeSpan.FromMinutes(90);

            BinaryPrefs.SetTimeSpan("key", value);

            BinaryPrefs.GetTimeSpan("key").Should().Be(value);
        }

        [Test]
        public void SetVector2_GetVector2_ShouldRoundTrip()
        {
            BinaryPrefs.SetVector2("key", new Vector2(1f, 2f));

            BinaryPrefs.GetVector2("key").Should().Be(new Vector2(1f, 2f));
        }

        [Test]
        public void SetVector3_GetVector3_ShouldRoundTrip()
        {
            BinaryPrefs.SetVector3("key", new Vector3(1f, 2f, 3f));

            BinaryPrefs.GetVector3("key").Should().Be(new Vector3(1f, 2f, 3f));
        }

        [Test]
        public void SetVector4_GetVector4_ShouldRoundTrip()
        {
            BinaryPrefs.SetVector4("key", new Vector4(1f, 2f, 3f, 4f));

            BinaryPrefs.GetVector4("key").Should().Be(new Vector4(1f, 2f, 3f, 4f));
        }

        [Test]
        public void SetVector2Int_GetVector2Int_ShouldRoundTrip()
        {
            BinaryPrefs.SetVector2Int("key", new Vector2Int(1, 2));

            BinaryPrefs.GetVector2Int("key").Should().Be(new Vector2Int(1, 2));
        }

        [Test]
        public void SetVector3Int_GetVector3Int_ShouldRoundTrip()
        {
            BinaryPrefs.SetVector3Int("key", new Vector3Int(1, 2, 3));

            BinaryPrefs.GetVector3Int("key").Should().Be(new Vector3Int(1, 2, 3));
        }

        [Test]
        public void SetQuaternion_GetQuaternion_ShouldRoundTrip()
        {
            BinaryPrefs.SetQuaternion("key", new Quaternion(1f, 2f, 3f, 4f));

            BinaryPrefs.GetQuaternion("key").Should().Be(new Quaternion(1f, 2f, 3f, 4f));
        }

        [Test]
        public void GetLong_ShouldReturnDefaultWhenKeyStoredAsInt()
        {
            BinaryPrefs.SetInt("key", 42);

            BinaryPrefs.GetLong("key", 7L).Should().Be(7L);
        }

        [Test]
        public void SetGeneric_GetGeneric_ShouldRoundTrip()
        {
            BinaryPrefs.Set("key", 42.125d);

            BinaryPrefs.Get<double>("key").Should().Be(42.125d);
        }

        [Test]
        public void GetGeneric_ShouldReturnDefaultWhenTypeDoesNotMatch()
        {
            BinaryPrefs.SetInt("key", 42);

            BinaryPrefs.Get("key", 7d).Should().Be(7d);
        }

        [Test]
        public void SetGeneric_ShouldThrowForUnregisteredType()
        {
            FluentActions.Invoking(() => BinaryPrefs.Set("key", new object())).Should().Throw<UnregisteredTypeException>();
        }

        [Test]
        public void TypeOf_ShouldReturnNullForUnknownKey()
        {
            BinaryPrefs.TypeOf("unknownKey").Should().BeNull();
        }
    }

    [TestFixture]
    public class BinaryPrefsEnumTests : BinaryPrefsTestsBase
    {
        public enum IntBacked
        {
            None = 0,
            Second = 2,
            Negative = -5
        }

        public enum ByteBacked : byte
        {
            None = 0,
            Max = byte.MaxValue
        }

        public enum ULongBacked : ulong
        {
            None = 0,
            Max = ulong.MaxValue
        }

        [TestCase(IntBacked.Second)]
        [TestCase(IntBacked.Negative)]
        [TestCase(IntBacked.None)]
        public void SetEnum_GetEnum_ShouldRoundTripIntBackedEnum(IntBacked value)
        {
            BinaryPrefs.SetEnum("key", value);

            BinaryPrefs.GetEnum<IntBacked>("key").Should().Be(value);
        }

        [Test]
        public void SetEnum_GetEnum_ShouldRoundTripByteBackedEnum()
        {
            BinaryPrefs.SetEnum("key", ByteBacked.Max);

            BinaryPrefs.GetEnum<ByteBacked>("key").Should().Be(ByteBacked.Max);
        }

        [Test]
        public void SetEnum_GetEnum_ShouldRoundTripULongBackedEnum()
        {
            BinaryPrefs.SetEnum("key", ULongBacked.Max);

            BinaryPrefs.GetEnum<ULongBacked>("key").Should().Be(ULongBacked.Max);
        }

        [Test]
        public void GetEnum_ShouldReturnDefaultWhenKeyNotFound()
        {
            BinaryPrefs.GetEnum("unknownKey", IntBacked.Second).Should().Be(IntBacked.Second);
        }

        [Test]
        public void GetEnum_ShouldReturnDefaultWhenKeyStoredAsInt()
        {
            BinaryPrefs.SetInt("key", 2);

            BinaryPrefs.GetEnum("key", IntBacked.Negative).Should().Be(IntBacked.Negative);
        }
    }
}
