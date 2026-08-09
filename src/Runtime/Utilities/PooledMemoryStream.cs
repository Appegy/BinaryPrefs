using System;
using System.Buffers;
using System.IO;

namespace Appegy.Storage
{
    internal sealed class PooledMemoryStream : Stream
    {
        private const int MinimumCapacity = 1024;
        private const int MaximumRememberedCapacity = 1024 * 1024;

        private byte[] _buffer = Array.Empty<byte>();
        private int _rememberedCapacity = MinimumCapacity;
        private int _position;
        private int _length;

        public int Capacity => _buffer.Length;

        public byte[] GetBuffer() => _buffer;

        public void Reset()
        {
            if (_buffer.Length < _rememberedCapacity)
            {
                Release();
                _buffer = ArrayPool<byte>.Shared.Rent(_rememberedCapacity);
            }
            _position = 0;
            _length = 0;
        }

        public void Release()
        {
            _rememberedCapacity = Math.Clamp(Math.Max(_rememberedCapacity, _length), MinimumCapacity, MaximumRememberedCapacity);
            if (_buffer.Length > 0)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
                _buffer = Array.Empty<byte>();
            }
            _position = 0;
            _length = 0;
        }

        public override bool CanRead => false;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _length)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, $"Position must be within [0, {_length}].");
                }
                _position = (int)value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(PooledMemoryStream)} is write-only.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new UnexpectedEnumException(typeof(SeekOrigin), origin)
            };
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException($"{nameof(PooledMemoryStream)} length is defined by written data.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCapacity(_position + count);
            Array.Copy(buffer, offset, _buffer, _position, count);
            Advance(count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            EnsureCapacity(_position + buffer.Length);
            buffer.CopyTo(new Span<byte>(_buffer, _position, buffer.Length));
            Advance(buffer.Length);
        }

        public override void WriteByte(byte value)
        {
            EnsureCapacity(_position + 1);
            _buffer[_position] = value;
            Advance(1);
        }

        protected override void Dispose(bool disposing)
        {
            Release();
            base.Dispose(disposing);
        }

        private void EnsureCapacity(int required)
        {
            if (required < 0)
            {
                throw new IOException("Storage is too big to be serialized: buffer would exceed 2 GB.");
            }
            if (required <= _buffer.Length)
            {
                return;
            }
            var doubled = (int)Math.Min((long)_buffer.Length * 2, int.MaxValue);
            var grown = ArrayPool<byte>.Shared.Rent(Math.Max(Math.Max(required, doubled), MinimumCapacity));
            Array.Copy(_buffer, grown, _length);
            if (_buffer.Length > 0)
            {
                ArrayPool<byte>.Shared.Return(_buffer);
            }
            _buffer = grown;
        }

        private void Advance(int count)
        {
            _position += count;
            if (_position > _length)
            {
                _length = _position;
            }
        }
    }
}
