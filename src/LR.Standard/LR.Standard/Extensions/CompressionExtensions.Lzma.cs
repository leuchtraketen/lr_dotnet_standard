using System;
using System.IO;

namespace LR.Standard;

public static partial class StringExtensions
{
    public static void ToLzma(this Stream stream_input, Stream stream_output, long length_input)
    {
        _toLzma(stream_input, stream_output, length_input: length_input);
    }

    private static void _toLzma(this Stream stream_input, Stream stream_output, long length_input)
    {
        SevenZip.Compression.LZMA.Encoder coder = new();

        coder.WriteCoderProperties(stream_output);

        for (int i = 0; i < 8; i++)
        {
            stream_output.WriteByte((byte)( ( (long)length_input ) >> ( 8 * i ) ));
        }

        coder.Code(stream_input, stream_output, progress: null);
    }

    public static void FromLzma(this Stream stream_input, Stream stream_output)
    {
        _fromLzma(stream_input, stream_output);
    }

    private static void _fromLzma(Stream stream_input, Stream stream_output)
    {
        SevenZip.Compression.LZMA.Decoder coder = new();

        // Read the decoder properties
        byte[] properties = new byte [5];
        stream_input.Read(properties, 0, 5);

        // Read in the decompress file size.
        byte[] file_length_nytes = new byte [8];
        stream_input.Read(file_length_nytes, 0, 8);
        long file_length = BitConverter.ToInt64(file_length_nytes, 0);

        coder.SetDecoderProperties(properties);
        coder.Code(stream_input, stream_output, size_output: file_length);
    }

    public static void ToLzma(this byte[] input, out byte[] out_output)
    {
        _toLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToLzma(this Memory<byte> input, out byte[] out_output)
    {
        _toLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void ToLzma(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _toLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _toLzma(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        using MemoryStream stream_output = new();

        unsafe
        {
            fixed (byte* data_ptr = &input.Span[0])
            {
                using UnmanagedMemoryStream stream_input = new(data_ptr, input.Length);
                stream_input.ToLzma(stream_output, length_input: input.Length);
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }

    public static void FromLzma(this byte[] input, out byte[] out_output)
    {
        _fromLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromLzma(this Memory<byte> input, out byte[] out_output)
    {
        _fromLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    public static void FromLzma(this ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        _fromLzma(input: (ReadOnlyMemory<byte>)input, out_output: out out_output);
    }

    private static void _fromLzma(ReadOnlyMemory<byte> input, out byte[] out_output)
    {
        using MemoryStream stream_output = new();

        unsafe
        {
            fixed (byte* data_ptr = &input.Span[0])
            {
                using UnmanagedMemoryStream stream_input = new(data_ptr, input.Length);
                _fromLzma(stream_input, stream_output);
            }
        }

        byte[] bytes_output = stream_output.ToArray();
        out_output = bytes_output;
    }
}
