using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Appegy.Storage.Example
{
    public static class MenuItemsTools
    {
        private static readonly string[] _mutableFolders =
        {
            "Assets",
            "Packages",
            "ProjectSettings",
        };

        private static readonly HashSet<Type> _textAssetsTypes = new HashSet<Type>
        {
            typeof(MonoScript),
            typeof(DefaultAsset),
            typeof(TextAsset),
            typeof(AssemblyDefinitionAsset),
            typeof(AssemblyDefinitionReferenceAsset)
        };

        [MenuItem("Assets/Reserialize All", false, 40)]
        public static void ReserializeAll()
        {
            ReserializeAssets(AssetDatabase.GetAllAssetPaths());
        }

        private static void ReserializeAssets(IEnumerable<string> assets)
        {
            var regularAssets = new HashSet<string>();
            var textAssets = new HashSet<string>();

            foreach (var assetPath in assets.Where(IsMutableAsset))
            {
                if (_textAssetsTypes.Contains(AssetDatabase.GetMainAssetTypeAtPath(assetPath)))
                {
                    textAssets.Add(assetPath);
                }
                else
                {
                    regularAssets.Add(assetPath);
                }
            }

            AssetDatabase.StartAssetEditing();
            ForceReserialize(regularAssets, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            ForceReserialize(textAssets, ForceReserializeAssetsOptions.ReserializeAssets);
            AssetDatabase.SaveAssets();
            AssetDatabase.StopAssetEditing();
        }

        private static void ForceReserialize(IEnumerable<string> assetsCollection, ForceReserializeAssetsOptions options)
        {
            AssetDatabase.ForceReserializeAssets(assetsCollection, options);
        }

        private static bool IsMutableAsset(string assetPath)
        {
            var realPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), assetPath);
            return _mutableFolders.Any(c => realPath.StartsWith(c));
        }
    }
}