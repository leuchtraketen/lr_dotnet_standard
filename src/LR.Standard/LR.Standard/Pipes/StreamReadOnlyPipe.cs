using System;
using System.IO;

namespace LR.Standard.Pipes;

internal sealed class StreamReadOnlyPipe : IReadOnlyPipe
{
    private readonly Stream _stream;

    public StreamReadOnlyPipe(Stream stream)
    {
        this._stream = stream;
    }

    public void Read(out byte out_byte, out bool content_available, out bool end_reached)
    {
        int b = this._stream.ReadByte();
        if (b == -1)
        {
            out_byte = 0;
            end_reached = true;
            content_available = false;
        }
        else
        {
            out_byte = (byte)b;
            end_reached = false;
            content_available = true;
        }
    }

#if NET471 || NETSTANDARD2_0 || NETCOREAPP2_0
        private static int _stream_read_legacy (Stream stream, Span<byte> out_bytes)
        {
            byte [] rented_buffer = ArrayPool<byte>.Shared.Rent (out_bytes.Length);
            try
            {
                int num_actually_read = stream.Read (rented_buffer, 0, out_bytes.Length);
                if ((uint)num_actually_read > (uint)out_bytes.Length)
                {
                    throw new IOException ("StreamTooLong");
                }
                new Span<byte> (rented_buffer, 0, num_actually_read).CopyTo (out_bytes);
                return num_actually_read;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return (rented_buffer);
            }
        }

        public void Read (Span<byte> out_bytes, out int out_count_bytes_actually_read, out bool out_end_reached)
        {
            out_count_bytes_actually_read = _stream_read_legacy (_stream, out_bytes);
            out_end_reached = out_count_bytes_actually_read == 0;
        }

#else

    public void Read(Span<byte> out_bytes, out int out_count_bytes_actually_read, out bool out_end_reached)
    {
        out_count_bytes_actually_read = this._stream.Read(out_bytes);
        out_end_reached = out_count_bytes_actually_read == 0;
    }

#endif
}
