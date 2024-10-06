using System.IO;
using UnityEditor;
using UnityEngine;
using static Appegy.Storage.PackageInfo;

namespace Appegy.Storage.Example
{
    public static class MenuItems
    {
        public const string RootMenuItem = "Appegy/";

        [MenuItem(RootMenuItem + "Reveal in Explorer")]
        public static void OpenPersistentFolder()
        {
            var cacheFolder = PersistentFolder;
            if (Directory.Exists(cacheFolder))
            {
                EditorUtility.RevealInFinder(cacheFolder);
            }
            else
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        [MenuItem(RootMenuItem + "Delete all data")]
        public static void ClearPersistentFolder()
        {
            var cacheFolder = PersistentFolder;
            if (Directory.Exists(cacheFolder))
            {
                Directory.Delete(cacheFolder, true);
            }
        }

        [MenuItem(RootMenuItem + "Reserialize Project", priority = 100)]
        [MenuItem("Assets/Reserialize All", false, 40)]
        public static void ReserializeAll()
        {
            ReserializationUtilities.ReserializeAssets(AssetDatabase.GetAllAssetPaths());
        }
    }
}