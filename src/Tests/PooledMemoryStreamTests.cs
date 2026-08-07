using System;
using System.Buffers;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class PooledMemoryStreamTests
    {
        [Test]
        public void WhenWriteFitsIntoCapacity_ThenBufferNotReallocated()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(1024);
            var buffer = stream.GetBuffer();
            var capacity = stream.Capacity;

            stream.Write(new byte[capacity], 0, capacity);

            stream.GetBuffer().Should().BeSameAs(buffer);
            stream.Capacity.Should().Be(capacity);
            stream.Release();
        }

        [Test]
        public void WhenWriteExceedsCapacity_ThenBufferGrownAndDataPreserved()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(256);
            var capacity = stream.Capacity;
            var payload = new byte[capacity + 100];
            for (var i = 0; i < payload.Length; i++)
            {
                payload[i] = (byte)(i % 251);
            }

            stream.Write(payload, 0, capacity);
            stream.Write(payload, capacity, 100);

            stream.Capacity.Should().BeGreaterThan(capacity);
            stream.Length.Should().Be(payload.Length);
            stream.GetBuffer().AsSpan(0, payload.Length).ToArray().Should().Equal(payload);
            stream.Release();
        }

        [Test]
        public void WhenReleased_ThenBufferReturnedToPool()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(1024);
            var buffer = stream.GetBuffer();

            stream.Release();

            stream.Capacity.Should().Be(0);
            var rented = ArrayPool<byte>.Shared.Rent(1024);
            rented.Should().BeSameAs(buffer);
            ArrayPool<byte>.Shared.Return(rented);
        }

        [Test]
        public void WhenPositionMovedBack_ThenWriteOverwritesWithoutTruncating()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(64);
            stream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            stream.Position = 1;
            stream.Write(new byte[] { 9 }, 0, 1);
            stream.Position = stream.Length;

            stream.Length.Should().Be(4);
            stream.Position.Should().Be(4);
            stream.GetBuffer().AsSpan(0, 4).ToArray().Should().Equal(new byte[] { 1, 9, 3, 4 });
            stream.Release();
        }

        [Test]
        public void WhenPositionSetBeyondLength_ThenThrows()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(64);
            stream.Write(new byte[] { 1, 2 }, 0, 2);

            Action action = () => stream.Position = 3;

            action.Should().Throw<ArgumentOutOfRangeException>();
            stream.Release();
        }

        [Test]
        public void WhenRead_ThenThrows()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(64);

            Action action = () => stream.Read(new byte[4], 0, 4);

            action.Should().Throw<NotSupportedException>();
            stream.Release();
        }

        [Test]
        public void WhenReset_ThenPositionAndLengthAreZero()
        {
            var stream = new PooledMemoryStream();
            stream.Reset(64);
            stream.Write(new byte[] { 1, 2, 3 }, 0, 3);

            stream.Reset(64);

            stream.Length.Should().Be(0);
            stream.Position.Should().Be(0);
            stream.Release();
        }
    }
}
