using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace LR.Standard;

public static partial class StringExtensions
{
    public static void ToGzipBase64(this string input, out string out_output)
    {
        using MemoryStream stream_input = new(Encoding.UTF8.GetBytes(input));
        using MemoryStream stream_output = new();
        using (GZipStream stream_output_gzipcompress = new(stream_output, CompressionMode.Compress, true))
        {
            stream_input.CopyTo(stream_output_gzipcompress);
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = Convert.ToBase64String(bytes_output);
    }

    public static void ToGzipBase64(this byte[] input, out string out_output)
    {
        using MemoryStream stream_input = new(input);
        using MemoryStream stream_output = new();
        using (GZipStream stream_output_gzipcompress = new(stream_output, CompressionMode.Compress, true))
        {
            stream_input.CopyTo(stream_output_gzipcompress);
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = Convert.ToBase64String(bytes_output);
    }

    public static void FromGzipBase64(this string input, out string out_output)
    {
        using MemoryStream stream_input = new(Convert.FromBase64String(input));
        using MemoryStream stream_output = new();
        using (GZipStream stream_input_gzipdecompress = new(stream_input, CompressionMode.Decompress))
        {
            stream_input_gzipdecompress.CopyTo(stream_output);
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = Encoding.UTF8.GetString(bytes_output);
    }

    public static void FromGzipBase64(this string input, out byte[] out_output)
    {
        using MemoryStream stream_input = new(Convert.FromBase64String(input));
        using MemoryStream stream_output = new();
        using (GZipStream stream_input_gzipdecompress = new(stream_input, CompressionMode.Decompress))
        {
            stream_input_gzipdecompress.CopyTo(stream_output);
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }

    public static void FromGzip(this byte[] input, out byte[] out_output)
    {
        _fromGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromGzip(this Memory<byte> input, out byte[] out_output)
    {
        _fromGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromGzip(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _fromGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _fromGzip(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        if (input.Length == 0)
        {
            out_output = Array.Empty<byte>();
            return;
        }

        using MemoryStream stream_output = new();

        unsafe
        {
            fixed (byte* data_ptr = &input.Span[0])
            {
                using (UnmanagedMemoryStream stream_input = new(data_ptr, input.Length))
                {
                    using (GZipStream stream_input_gzipdecompress = new(stream_input, CompressionMode.Decompress))
                    {
                        stream_input_gzipdecompress.CopyTo(stream_output);
                    }
                }
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }

    public static void ToGzip(this byte[] input, out byte[] out_output)
    {
        _toGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToGzip(this Memory<byte> input, out byte[] out_output)
    {
        _toGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToGzip(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _toGzip(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _toGzip(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        if (input.Length == 0)
        {
            out_output = Array.Empty<byte>();
            return;
        }

        using MemoryStream stream_output = new();

        unsafe
        {
            fixed (byte* data_ptr = &input.Span[0])
            {
                using (UnmanagedMemoryStream stream_input = new(data_ptr, input.Length))
                {
                    using (GZipStream stream_output_gzipcompress = new(stream_output, CompressionMode.Compress, true))
                    {
                        stream_input.CopyTo(stream_output_gzipcompress);
                    }
                }
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }
}
