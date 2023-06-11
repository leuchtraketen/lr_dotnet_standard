using System;
using System.IO;
using System.Threading;

namespace LR.Standard.Pipes;

public interface IReadOnlyPipe
{
    void Read(out byte out_byte, out bool content_available, out bool end_reached);
    void Read(Span<byte> out_bytes, out int out_count_bytes_actually_read, out bool out_end_reached);
}

public interface IWriteOnlyPipe
{
    void Write(byte[] bytes);
    void Write(ReadOnlySpan<byte> bytes);
    void Write(byte b);
}

public interface IPipe : IReadOnlyPipe, IWriteOnlyPipe
{
}

public static class PipeExtensions
{
    public static Stream ToReadOnlyStream(this IReadOnlyPipe that, Action check_for_exceptions)
    {
        return new ReadOnlyPipeStream(that, check_for_exceptions: check_for_exceptions);
    }

    public static Stream ToReadOnlyStream(this IReadOnlyPipe that)
    {
        return new ReadOnlyPipeStream(that, check_for_exceptions: null);
    }

    public static IReadOnlyPipe ToReadOnlyPipe(this Stream that)
    {
        return new StreamReadOnlyPipe(that);
    }

    public static int ReadByteBlocking(this IReadOnlyPipe that)
    {
        int i = 0;
        do
        {
            that.Read(out byte b, out bool content_available, out bool end_reached);
            if (end_reached)
            {
                return -1;
            }

            if (content_available)
            {
                return b;
            }

            if (i++ > 3) Thread.Sleep(i > 100 ? 50 : 5);
        } while (true);
    }

    public static void ReadBytesBlocking(this IReadOnlyPipe that, Span<byte> out_bytes, int count_bytes_expected, out int out_count_bytes_actually_read, out bool out_end_reached)
    {
        out_count_bytes_actually_read = 0;
        out_end_reached = false;
        int i = 0;
        while (out_count_bytes_actually_read < count_bytes_expected)
        {
            that.Read(out_bytes.Slice(out_count_bytes_actually_read, count_bytes_expected - out_count_bytes_actually_read), out int count_bytes_actually_read_chunk,
                out out_end_reached);
            out_count_bytes_actually_read += count_bytes_actually_read_chunk;

            if (out_end_reached)
            {
                return;
            }

            if (i++ > 3) Thread.Sleep(i > 100 ? 50 : 5);
        }
    }

    public static void CopyTo(this IReadOnlyPipe that, Stream stream_output, Func<bool>? check_if_cancelled = null, Action<long>? handle_progress = null)
    {
        long total_bytes_read = 0;
        const int buffer_size = 128 * 1024;
        Span<byte> buffer = new byte [buffer_size];
        while (true)
        {
            that.Read(buffer, out int count_bytes_read, out bool end_reached);

            if (count_bytes_read != 0)
            {
                if (check_if_cancelled is Func<bool> f && f())
                {
                    return;
                }

                total_bytes_read += count_bytes_read;
                handle_progress?.Invoke(total_bytes_read);

                stream_output.Write(buffer.Slice(0, count_bytes_read));
            }
            else
            {
                Thread.Sleep(20);
            }

            if (end_reached)
            {
                Console.WriteLine("END REACHED");
                break;
            }
        }
    }
}
