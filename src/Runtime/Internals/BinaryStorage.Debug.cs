#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Appegy.Storage
{
    public partial class BinaryStorage
    {
        private static readonly HashSet<string> _lockedFiles = new();

        static partial void ThrowIfFilePathLocked(string filePath)
        {
            if (_lockedFiles.Contains(StorageFile.Normalize(filePath)))
            {
                throw new Exception($"Storage already opened by this path. File path: {filePath}");
            }
        }

        static partial void LockFilePathInEditor(string filePath)
        {
            _lockedFiles.Add(StorageFile.Normalize(filePath));
        }

        static partial void UnlockFilePathInEditor(string filePath)
        {
            _lockedFiles.Remove(StorageFile.Normalize(filePath));
        }
    }
}

#endif
