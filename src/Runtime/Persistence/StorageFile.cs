using System.Collections.Concurrent;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Appegy.Storage
{
    internal sealed class StorageFile
    {
        internal const string TempFileExtension = ".tmp";
        internal const string BackupFileExtension = ".bak";
        internal const string DebugJsonFileExtension = ".json";

        private const int NoBuffering = 1;

        private static readonly ConcurrentDictionary<string, StorageFile> _files = new();
        private static readonly UTF8Encoding _debugJsonEncoding = new(false);

        internal delegate bool ReadAttempt(string filePath, out StorageFileCorruptedException failure);

        public readonly string Main;
        public readonly string Temp;
        public readonly string Backup;
        public readonly string DebugJson;

        private readonly object _publishGate = new();

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

        public static string Normalize(string filePath)
        {
            return Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar);
        }

        public void Publish(StorageSnapshot snapshot)
        {
            try
            {
                lock (_publishGate)
                {
                    if (snapshot.IsEmpty)
                    {
                        RemoveFiles();
                        return;
                    }

                    WriteTemp(snapshot);
                    if (File.Exists(Main))
                    {
                        File.Replace(Temp, Main, Backup);
                    }
                    else
                    {
                        File.Move(Temp, Main);
                    }
                }
            }
            finally
            {
                snapshot.Release();
            }
        }

        public void Remove()
        {
            lock (_publishGate)
            {
                RemoveFiles();
            }
        }

        public void WriteDebugJson(string json)
        {
            EnsureDirectoryExists();
            File.WriteAllText(DebugJson, json, _debugJsonEncoding);
        }

        public void RemoveDebugJson()
        {
            DeleteFileIfExists(DebugJson);
        }

        public void Load(ReadAttempt tryRead)
        {
            lock (_publishGate)
            {
                if (!File.Exists(Main))
                {
                    return;
                }

                DeleteFileIfExists(Temp);

                if (tryRead(Main, out var failure))
                {
                    return;
                }

                DeleteFileIfExists(Main);

                if (File.Exists(Backup))
                {
                    File.Move(Backup, Main);
                    if (tryRead(Main, out _))
                    {
                        return;
                    }
                    DeleteFileIfExists(Main);
                }

                ExceptionDispatchInfo.Capture(failure).Throw();
            }
        }

        private void WriteTemp(StorageSnapshot snapshot)
        {
            EnsureDirectoryExists();
            using var stream = new FileStream(Temp, FileMode.Create, FileAccess.Write, FileShare.None, NoBuffering);
            stream.Write(snapshot.Buffer, 0, snapshot.Length);
            stream.Flush(true);
        }

        private void RemoveFiles()
        {
            DeleteFileIfExists(Main);
            DeleteFileIfExists(Temp);
            DeleteFileIfExists(Backup);
        }

        private void EnsureDirectoryExists()
        {
            var directoryName = Path.GetDirectoryName(Main);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
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
