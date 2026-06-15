using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Superb.Core.Editor
{
    public static class ReserializeAssetsTool
    {
        // Importers that flood meta with default fields when reserialized.
        // Verified empirically on Unity 6000.4.6f1: MonoImporter adds executionOrder/icon/
        // defaultReferences/etc., PluginImporter adds the full platformData matrix.
        // All other importers either don't touch meta or migrate legacy fields to the
        // canonical format (e.g. TextScriptImporter drops timeCreated/licenseType).
        private static readonly HashSet<Type> _lazyMetaImporters = new()
        {
            typeof(MonoImporter),
            typeof(PluginImporter),
        };

        [MenuItem("Assets/Reserialize/Project")]
        public static void ReserializeProject()
        {
            Reserialize(includeProject: true, includeLocalPackages: false);
        }

        [MenuItem("Assets/Reserialize/Local Packages")]
        public static void ReserializeLocalPackages()
        {
            Reserialize(includeProject: false, includeLocalPackages: true);
        }

        private static void Reserialize(bool includeProject, bool includeLocalPackages)
        {
            var projectRoot = Path.GetFullPath(Directory.GetCurrentDirectory());

            var withMetadata = new List<string>();
            var assetsOnly = new List<string>();

            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!ShouldProcess(path, includeProject, includeLocalPackages, projectRoot))
                {
                    continue;
                }

                if (!File.Exists(path + ".meta"))
                {
                    // ProjectSettings/*.asset have no .meta — reserialize the asset only.
                    assetsOnly.Add(path);
                    continue;
                }

                var importer = AssetImporter.GetAtPath(path);
                if (importer != null && _lazyMetaImporters.Contains(importer.GetType()))
                {
                    assetsOnly.Add(path);
                }
                else
                {
                    withMetadata.Add(path);
                }
            }

            Debug.Log($"[Reserialize] {withMetadata.Count} with metadata + {assetsOnly.Count} assets only");

            try
            {
                AssetDatabase.ForceReserializeAssets(withMetadata, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
                AssetDatabase.ForceReserializeAssets(assetsOnly, ForceReserializeAssetsOptions.ReserializeAssets);
            }
            finally
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static bool ShouldProcess(string path, bool includeProject, bool includeLocalPackages, string projectRoot)
        {
            if (path.StartsWith("Assets/"))
            {
                return includeProject;
            }

            if (path.StartsWith("ProjectSettings/"))
            {
                return includeProject;
            }

            if (!path.StartsWith("Packages/"))
            {
                return false;
            }

            if (!includeLocalPackages)
            {
                return false;
            }

            var info = PackageInfo.FindForAssetPath(path);
            if (info == null)
            {
                return false;
            }

            // Embedded packages live inside the project's Packages/ folder — always include.
            if (info.source == PackageSource.Embedded)
            {
                return true;
            }

            // Local (file:) packages may resolve outside the project — typically shared SDK
            // submodules. Skip those so reserialization doesn't write into another repository.
            if (info.source != PackageSource.Local)
            {
                return false;
            }

            var resolved = Path.GetFullPath(info.resolvedPath);
            return resolved.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase);
        }
    }
}
