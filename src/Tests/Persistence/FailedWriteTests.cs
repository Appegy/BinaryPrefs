using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Appegy.Storage
{
    public class FailedWriteTests : BaseStorageTests
    {
        [TearDown]
        public void UnblockWrites()
        {
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath);
            }
        }

        [Test]
        public void WhenBackgroundWriteFails_ThenDisposeWritesTheChange()
        {
            var storage = Open(autoSave: true);
            Directory.CreateDirectory(TempPath);
            using (var writeFailed = new ManualResetEventSlim())
            {
                void OnLog(string message, string stackTrace, LogType type)
                {
                    if (type == LogType.Error && message.StartsWith("Failed to save storage"))
                    {
                        writeFailed.Set();
                    }
                }

                LogAssert.Expect(LogType.Error, new Regex("Failed to save storage"));
                Application.logMessageReceivedThreaded += OnLog;
                storage.Set("value", 7);
                var failed = writeFailed.Wait(TimeSpan.FromSeconds(10));
                Application.logMessageReceivedThreaded -= OnLog;
                failed.Should().BeTrue();
            }

            Directory.Delete(TempPath);
            storage.Dispose();

            using var reopened = Open();
            reopened.Get<int>("value").Should().Be(7);
        }

        [Test]
        public void WhenAutoSaveWriteFails_ThenDisposeWritesTheChange()
        {
            var storage = Open(autoSave: true, saveOnBackgroundThread: false);
            Directory.CreateDirectory(TempPath);

            storage.Invoking(s => s.Set("value", 7)).Should().Throw<Exception>();

            Directory.Delete(TempPath);
            storage.Dispose();

            using var reopened = Open();
            reopened.Get<int>("value").Should().Be(7);
        }
    }
}
