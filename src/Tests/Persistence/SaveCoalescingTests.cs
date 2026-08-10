using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class SaveCoalescingTests : BaseStorageTests
    {
        private const int BurstSize = 200;
        private const int WaitTimeoutMs = 10000;
        private const int PollIntervalMs = 5;
        private const int WriterSettleMs = 200;

        [Test]
        public void WhenWriterIsIdle_ThenChangeIsSerializedRightAway()
        {
            using var storage = Open(autoSave: true);

            storage.Set("value", 1);

            storage.SerializeCount.Should().Be(1);
        }

        [Test]
        public void WhenPreviousSnapshotIsStillOnItsWay_ThenChangesAreCoalesced()
        {
            using var storage = Open(autoSave: true);

            Burst(storage);

            storage.SerializeCount.Should().BeLessThan(BurstSize);
        }

        [Test]
        public void WhenBurstCoalesced_ThenExplicitSavePutsTheLastStateOnDisk()
        {
            using var storage = Open(autoSave: true);

            Burst(storage);
            storage.Save();

            ReadValueFromDisk().Should().Be(BurstSize);
        }

        [Test]
        public void WhenChangesDeferred_ThenTheyReachDiskWithoutExplicitSave()
        {
            var context = new PumpableSynchronizationContext();
            using var storage = OpenWith(context);
            Burst(storage);
            var serializedDuringBurst = storage.SerializeCount;

            PumpUntil(context, () => storage.SerializeCount > serializedDuringBurst, "deferred changes were never serialized again");

            WaitUntilWriterIsQuiet();
            ReadValueFromDisk().Should().Be(BurstSize);
        }

        [Test]
        public void WhenDeferredChangesAreStillThereOnDispose_ThenTheyReachDisk()
        {
            using (var storage = Open(autoSave: true))
            {
                Burst(storage);
            }

            ReadValueFromDisk().Should().Be(BurstSize);
        }

        [Test]
        public void WhenThereIsNoSynchronizationContext_ThenEveryChangeIsSerialized()
        {
            using var storage = OpenWith(null);

            for (var i = 1; i <= 5; i++)
            {
                storage.Set("value", i);
            }

            storage.SerializeCount.Should().Be(5);
        }

        [Test]
        public void WhenChangeScopeIsOpen_ThenDeferredSaveWaitsForItsEnd()
        {
            var context = new PumpableSynchronizationContext();
            using var storage = OpenWith(context);
            storage.Set("value", 1);
            storage.Set("value", 2);
            WaitUntilWriterIsQuiet();
            var serializedBeforeScope = storage.SerializeCount;

            using (storage.MultipleChangeScope())
            {
                storage.Set("value", 3);
                context.Pump();

                storage.SerializeCount.Should().Be(serializedBeforeScope);
            }

            storage.SerializeCount.Should().Be(serializedBeforeScope + 1);
            storage.Save();
            ReadValueFromDisk().Should().Be(3);
        }

        [Test]
        public void WhenBackgroundWriterDisabled_ThenNothingIsDeferred()
        {
            using var storage = Open(autoSave: true, saveOnBackgroundThread: false);

            for (var i = 1; i <= 5; i++)
            {
                storage.Set("value", i);
            }

            storage.SerializeCount.Should().Be(5);
            ReadValueFromDisk().Should().Be(5);
        }

        private BinaryStorage OpenWith(SynchronizationContext context)
        {
            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                return Open(autoSave: true);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previous);
            }
        }

        private static void Burst(BinaryStorage storage)
        {
            for (var i = 1; i <= BurstSize; i++)
            {
                storage.Set("value", i);
            }
        }

        private static void PumpUntil(PumpableSynchronizationContext context, Func<bool> condition, string message)
        {
            var deadline = Environment.TickCount + WaitTimeoutMs;
            while (Environment.TickCount < deadline)
            {
                context.Pump();
                if (condition())
                {
                    return;
                }
                Thread.Sleep(PollIntervalMs);
            }
            Assert.Fail(message);
        }

        private void WaitUntilWriterIsQuiet()
        {
            var deadline = Environment.TickCount + WaitTimeoutMs;
            while (Environment.TickCount < deadline && File.Exists(TempPath))
            {
                Thread.Sleep(PollIntervalMs);
            }
            Thread.Sleep(WriterSettleMs);
        }

        private sealed class PumpableSynchronizationContext : SynchronizationContext
        {
            private readonly ConcurrentQueue<KeyValuePair<SendOrPostCallback, object>> _posted = new();

            public override void Post(SendOrPostCallback callback, object state)
            {
                _posted.Enqueue(new KeyValuePair<SendOrPostCallback, object>(callback, state));
            }

            public void Pump()
            {
                while (_posted.TryDequeue(out var work))
                {
                    work.Key(work.Value);
                }
            }
        }
    }
}
