using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace Appegy.Storage
{
    /// <summary>
    /// The storage file on disk together with the companion files kept next to it. One instance per path, shared by every
    /// storage and every writer aiming at that path, so that publishing and loading are serialized across threads and a
    /// snapshot can never overwrite a newer one that already reached the disk.
    /// </summary>
    internal sealed class StorageFile
    {
        internal const string TempFileExtension = ".tmp";
        internal const string BackupFileExtension = ".bak";
        internal const string DebugJsonFileExtension = ".json";

        private static readonly ConcurrentDictionary<string, StorageFile> _files = new();
        private static readonly UTF8Encoding _debugJsonEncoding = new(false);

        public readonly string Main;
        public readonly string Temp;
        public readonly string Backup;
        public readonly string DebugJson;

        private readonly object _publishGate = new();
        private long _requestedGeneration;
        private long _publishedGeneration;
        private bool _directoryEnsured;

        private StorageFile(string mainFilePath)
        {
            Main = mainFilePath;
            Temp = mainFilePath + TempFileExtension;
            Backup = mainFilePath + BackupFileExtension;
            DebugJson = mainFilePath + DebugJsonFileExtension;
        }

        public static StorageFile Of(string filePath)
        {
            return _files.GetOrAdd(Normalize(filePath), path => new StorageFile(path));
        }

        /// <summary> The identity of a storage file: any two paths pointing at the same file normalize to the same string. </summary>
        public static string Normalize(string filePath)
        {
            return Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary> Stamp the next state of this file. Generations are monotonic per file, so a snapshot always knows whether it is newer than what is on disk. </summary>
        public long NextGeneration()
        {
            return Interlocked.Increment(ref _requestedGeneration);
        }

        /// <summary> Publish a snapshot as the storage file, atomically and durably. An empty snapshot removes the file, and a snapshot older than the published one is dropped. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        public void Publish(StorageSnapshot snapshot)
        {
            lock (_publishGate)
            {
                if (snapshot.Generation <= _publishedGeneration)
                {
                    return;
                }
                if (snapshot.IsEmpty)
                {
                    DeleteFileIfExists(Main);
                    DeleteFileIfExists(Temp);
                    DeleteFileIfExists(Backup);
                }
                else
                {
                    EnsureDirectoryExists();
                    using (var stream = new FileStream(Temp, FileMode.Create))
                    {
                        stream.Write(snapshot.Buffer, 0, snapshot.Length);
                        stream.Flush(true);
                    }
                    if (File.Exists(Main))
                    {
                        File.Replace(Temp, Main, Backup);
                    }
                    else
                    {
                        File.Move(Temp, Main);
                    }
                }
                _publishedGeneration = snapshot.Generation;
            }
        }

        /// <summary> Remove the storage file together with its companion files, and keep an older snapshot still in flight from bringing it back. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        public void Remove()
        {
            Publish(StorageSnapshot.Empty(NextGeneration()));
        }

        /// <summary> Write or remove the human-readable JSON copy kept next to the storage file. The copy is write-only and never loaded back. </summary>
        /// <param name="json"> The JSON to write, or null to remove the copy </param>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        public void WriteDebugJson(string json)
        {
            if (json == null)
            {
                DeleteFileIfExists(DebugJson);
                return;
            }
            EnsureDirectoryExists();
            File.WriteAllText(DebugJson, json, _debugJsonEncoding);
        }

        /// <summary> Load the storage file into memory. When it cannot be read, it is removed and the backup written by the previous publish takes its place. </summary>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        /// <exception cref="StorageFileCorruptedException"> Neither the storage file nor its backup could be read. Both are removed before this is thrown. </exception>
        /// <exception cref="KeyLoadFailedException"> A key failed to load and <paramref name="keyLoadFailedBehaviour"/> is <see cref="KeyLoadFailedBehaviour.ThrowException"/>. </exception>
        public void Load(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            StorageFileCorruptedException failure;
            lock (_publishGate)
            {
                failure = TryLoad(sections, data, keyLoadFailedBehaviour);
            }
            if (failure != null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }
        }

        private StorageFileCorruptedException TryLoad(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour)
        {
            ResetData(sections, data);

            if (!File.Exists(Main))
            {
                return null;
            }

            DeleteFileIfExists(Temp);

            if (TryReadFile(Main, sections, data, keyLoadFailedBehaviour, out var failure))
            {
                return null;
            }

            DeleteFileIfExists(Main);

            if (File.Exists(Backup))
            {
                File.Move(Backup, Main);
                if (TryReadFile(Main, sections, data, keyLoadFailedBehaviour, out _))
                {
                    return null;
                }
                DeleteFileIfExists(Main);
            }

            return failure;
        }

        private static bool TryReadFile(string filePath, IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour, out StorageFileCorruptedException failure)
        {
            try
            {
                StorageFormat.ReadFile(filePath, sections, data, keyLoadFailedBehaviour);
                failure = null;
                return true;
            }
            catch (StorageFileCorruptedException exception)
            {
                ResetData(sections, data);
                failure = exception;
                return false;
            }
        }

        private static void ResetData(IReadOnlyList<BinarySection> sections, Dictionary<string, Record> data)
        {
            data.Clear();
            for (var i = 0; i < sections.Count; i++)
            {
                sections[i].Count = 0;
            }
        }

        private void EnsureDirectoryExists()
        {
            if (_directoryEnsured)
            {
                return;
            }
            var directoryName = Path.GetDirectoryName(Main);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            _directoryEnsured = true;
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
