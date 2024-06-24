using System.IO;
using UnityEditor;
using UnityEngine;
using static Appegy.Storage.PackageInfo;

namespace Appegy.Storage.Example
{
    public static class MenuItems
    {
        public const string RootMenuItem = "Appegy/Persistent Folder/";

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
    }
}