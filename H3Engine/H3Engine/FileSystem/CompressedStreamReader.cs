using System;
using System.IO;
using System.IO.Compression;

namespace H3Engine.FileSystem
{
    public class CompressedStreamReader : IDisposable
    {
        private bool endOfFileReached = false;

        private MemoryStream buffer = null;

        private Stream decompressStream = null;

        private UInt64 ReadPosition
        {
            get; set;
        }

        private UInt64 WritePosition
        {
            get; set;
        }

        public CompressedStreamReader(Stream input, bool isGZip)
        {
            this.buffer = new MemoryStream();

            this.ReadPosition = 0;
            this.WritePosition = 0;

            if (isGZip)
            {
                this.decompressStream = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
            }
            else
            {
                // Raw deflate (zlib format) - skip 2-byte zlib header
                // zlib header is typically 0x78 0x01/0x5E/0x9C/0xDA
                int b1 = input.ReadByte();
                int b2 = input.ReadByte();
                if (b1 == -1 || b2 == -1)
                {
                    this.endOfFileReached = true;
                    this.decompressStream = null;
                    return;
                }

                // Check if this looks like a zlib header
                bool isZlibHeader = (b1 == 0x78) && ((b1 * 256 + b2) % 31 == 0);
                if (!isZlibHeader)
                {
                    // Not a zlib header, put bytes back by wrapping in a new stream
                    byte[] headerBytes = new byte[] { (byte)b1, (byte)b2 };
                    Stream remaining = input;
                    MemoryStream headerStream = new MemoryStream(headerBytes);
                    input = new ConcatenatedStream(headerStream, remaining);
                }

                this.decompressStream = new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true);
            }
        }

        public void Dispose()
        {
            if (this.decompressStream != null)
            {
                this.decompressStream.Dispose();
                this.decompressStream = null;
            }
        }

        public UInt64 Read(byte[] data, UInt64 size)
        {
            EnsureSize(this.ReadPosition + size);

            var toRead = Math.Min(size, (ulong)buffer.Length - this.ReadPosition);

            if (toRead > 0)
            {
                buffer.Seek((long)this.ReadPosition, SeekOrigin.Begin);
                buffer.Read(data, 0, (int)toRead);
                this.ReadPosition += toRead;
            }

            return toRead;
        }

        public void EnsureSize(UInt64 size)
        {
            while (buffer.Length < (long)size && !this.endOfFileReached)
            {
                var initialSize = buffer.Length;
                var currentStep = (long)size - initialSize;
                currentStep = (currentStep < 2048 ? 2048 : currentStep);
                UInt64 readSize = this.ReadMore((ulong)currentStep);

                if (readSize == 0)
                {
                    this.endOfFileReached = true;
                }
            }
        }

        public UInt64 GetSize()
        {
            return (UInt64)buffer.Length;
        }

        public void Reset()
        {
            this.ReadPosition = 0;
            this.WritePosition = 0;
            this.endOfFileReached = false;
        }

        /// <summary>
        /// Read more data into Buffer
        /// </summary>
        public UInt64 ReadMore(UInt64 size)
        {
            if (this.decompressStream == null)
            {
                return 0;
            }

            byte[] data = new byte[(int)size];
            int totalRead = 0;

            try
            {
                while (totalRead < (int)size)
                {
                    int bytesRead = this.decompressStream.Read(data, totalRead, (int)size - totalRead);
                    if (bytesRead == 0)
                    {
                        this.endOfFileReached = true;
                        break;
                    }
                    totalRead += bytesRead;
                }
            }
            catch (Exception)
            {
                this.endOfFileReached = true;
            }

            if (totalRead > 0)
            {
                buffer.Seek(0, SeekOrigin.End);
                buffer.Write(data, 0, totalRead);
            }

            if (this.endOfFileReached)
            {
                this.decompressStream.Dispose();
                this.decompressStream = null;
            }

            return (UInt64)totalRead;
        }

        public bool GetNextBlock()
        {
            if (this.decompressStream == null)
            {
                return false;
            }

            // Reset buffer for next block
            Reset();
            return true;
        }
    }

    /// <summary>
    /// Helper stream that concatenates two streams sequentially
    /// </summary>
    internal class ConcatenatedStream : Stream
    {
        private Stream first;
        private Stream second;
        private bool firstDone = false;

        public ConcatenatedStream(Stream first, Stream second)
        {
            this.first = first;
            this.second = second;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!firstDone)
            {
                int read = first.Read(buffer, offset, count);
                if (read > 0) return read;
                firstDone = true;
            }
            return second.Read(buffer, offset, count);
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
