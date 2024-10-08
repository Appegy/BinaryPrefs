﻿using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Appegy.BinaryStorage.Tests")]

namespace Appegy.Storage
{
    public static class PackageInfo
    {
        public const string Name = "com.appegy.binary-prefs";
        public const string Version = "1.0.0";
        public static string PersistentFolder => Path.Combine(Application.persistentDataPath, Name);
    }
}