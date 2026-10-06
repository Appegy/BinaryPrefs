using Appegy.Storage.Benchmarks;
using UnityEditor;
using UnityEngine;

namespace Appegy.Storage.Example.Editor
{
    internal static class StorageBenchMenu
    {
        [MenuItem("Tools/BinaryPrefs/Run storage bench")]
        private static void RunStorageBench()
        {
            StorageBench.RunAll(Application.temporaryCachePath);
        }
    }
}
