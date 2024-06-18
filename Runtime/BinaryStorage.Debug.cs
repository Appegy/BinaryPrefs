#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Appegy.Storage
{
    public partial class BinaryStorage
    {
        private static readonly HashSet<string> _lockedFiles = new();

        static partial void ThrowIfFilePathLocked(string filePath)
        {
            filePath = Path.GetFullPath(filePath).TrimEnd('\\');
            if (_lockedFiles.Any(s => filePath == s))
            {
                throw new Exception($"Storage already opened by this path. File path: {filePath}");
            }
        }

        static partial void LockFilePathInEditor(string filePath)
        {
            filePath = Path.GetFullPath(filePath).TrimEnd('\\');
            _lockedFiles.Add(filePath);
        }

        static partial void UnlockFilePathInEditor(string filePath)
        {
            filePath = Path.GetFullPath(filePath).TrimEnd('\\');
            _lockedFiles.Remove(filePath);
        }
    }
}

#endif