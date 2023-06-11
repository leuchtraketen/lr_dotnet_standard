using System;
using System.IO;

namespace LR.Standard;

public static partial class StringExtensions
{
    public static void ToBzip2(this Stream stream_input, Stream stream_output)
    {
        _toBzip2(stream_input, stream_output);
    }

    private static void _toBzip2(this Stream stream_input, Stream stream_output)
    {
        ICSharpCode.SharpZipLib.BZip2.BZip2.Compress(stream_input, stream_output, false, 9);
    }

    public static void FromBzip2(this Stream stream_input, Stream stream_output)
    {
        _fromBzip2(stream_input, stream_output);
    }

    private static void _fromBzip2(Stream stream_input, Stream stream_output)
    {
        ICSharpCode.SharpZipLib.BZip2.BZip2.Decompress(stream_input, stream_output, false);
    }

    public static void ToBzip2(this byte[] input, out byte[] out_output)
    {
        _toBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToBzip2(this Memory<byte> input, out byte[] out_output)
    {
        _toBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToBzip2(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _toBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _toBzip2(this ReadOnlyMemory<byte> input, out byte[] out_output)
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
                using UnmanagedMemoryStream stream_input = new(data_ptr, input.Length);
                stream_input.ToBzip2(stream_output);
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }

    public static void FromBzip2(this byte[] input, out byte[] out_output)
    {
        _fromBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromBzip2(this Memory<byte> input, out byte[] out_output)
    {
        _fromBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromBzip2(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _fromBzip2(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _fromBzip2(ReadOnlyMemory<byte> input, out byte[] out_output)
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
                using UnmanagedMemoryStream stream_input = new(data_ptr, input.Length);
                _fromBzip2(stream_input, stream_output);
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }
}
