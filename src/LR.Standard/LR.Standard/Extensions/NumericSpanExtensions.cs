using System;
using Microsoft.Extensions.Logging;

namespace LR.Standard;

public static class NumericSpanExtensions
{
    public static int ToInteger(this in ReadOnlySpan<byte> str, int default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        int result = 0;
        bool negative = false;
        for (int i = 0; i < str.Length; i++)
        {
            byte b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + ( b - '0' );
            else if (b == '-')
                negative = true;
            else
                return default_value;
        }

        if (negative) result = -result;
        return result;
    }

    public static int ToInteger(this in ReadOnlySpan<char> str, int default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        int result = 0;
        bool negative = false;
        for (int i = 0; i < str.Length; i++)
        {
            char b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + ( b - '0' );
            else if (b == '-')
                negative = true;
            else
                return default_value;
        }

        if (negative) result = -result;
        return result;
    }

    public static long ToLong(this in ReadOnlySpan<byte> str, long default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        long result = 0;
        bool negative = false;
        for (int i = 0; i < str.Length; i++)
        {
            byte b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + ( b - '0' );
            else if (b == '-')
                negative = true;
            else
                return default_value;
        }

        if (negative) result = -result;
        return result;
    }

    public static long ToLong(this in ReadOnlySpan<char> str, long default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        long result = 0;
        bool negative = false;
        for (int i = 0; i < str.Length; i++)
        {
            char b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + ( b - '0' );
            else if (b == '-')
                negative = true;
            else
                return default_value;
        }

        if (negative) result = -result;
        return result;
    }

    public static uint ToIntegerUnsigned(this in ReadOnlySpan<byte> str, uint default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        uint result = 0;
        for (int i = 0; i < str.Length; i++)
        {
            byte b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + (uint)( b - '0' );
            else
                return default_value;
        }

        return result;
    }

    public static uint ToIntegerUnsigned(this in ReadOnlySpan<char> str, uint default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        uint result = 0;
        for (int i = 0; i < str.Length; i++)
        {
            char b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + (uint)( b - '0' );
            else
                return default_value;
        }

        return result;
    }

    public static ulong ToLongUnsigned(this in ReadOnlySpan<byte> str, ulong default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        ulong result = 0;
        for (int i = 0; i < str.Length; i++)
        {
            byte b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + (uint)( b - '0' );
            else
                return default_value;
        }

        return result;
    }

    public static ulong ToLongUnsigned(this in ReadOnlySpan<char> str, ulong default_value = 0)
    {
        if (str.IsEmpty)
        {
            return default_value;
        }

        ulong result = 0;
        for (int i = 0; i < str.Length; i++)
        {
            char b = str[i];
            if (b >= '0' && b <= '9')
                result = result * 10 + (uint)( b - '0' );
            else
                return default_value;
        }

        return result;
    }

    public static ReadOnlySpan<byte> ToBinaryArray(this in ReadOnlySpan<byte> s, Span<byte> buffer, ILogger? logger)
    {
        int length = s.Length;
        int desired_length = (int)System.Math.Ceiling((double)length / 8.0);
        if (desired_length > buffer.Length)
        {
            logger?.LogError($"Buffer is too small: need {desired_length} bytes, but buffer has only {buffer.Length} bytes.");
            return ReadOnlySpan<byte>.Empty;
        }

        Span<byte> bytes = buffer.Slice(0, desired_length);
        int byteIndex, bitInByteIndex;
        byte mask;
        for (int bitIndex = 0; bitIndex < length; bitIndex++)
        {
            byteIndex = bitIndex / 8;
            bitInByteIndex = bitIndex % 8;
            mask = (byte)( 1 << bitInByteIndex );
            if (s[bitIndex] == '1')
            {
                bytes[byteIndex] |= mask;
            }
            else
            {
                bytes[byteIndex] &= (byte)~mask;
            }
        }

        return bytes;
    }

    public static ReadOnlySpan<bool> ToBitArray(this in ReadOnlySpan<byte> s, Span<bool> buffer, ILogger? logger)
    {
        int length = s.Length;
        if (length > buffer.Length)
        {
            logger?.LogError($"Buffer is too small: need {length} bytes, but buffer has only {buffer.Length} bytes.");
            return ReadOnlySpan<bool>.Empty;
        }

        Span<bool> bits = buffer.Slice(0, length);
        for (int bitIndex = 0; bitIndex < length; bitIndex++)
        {
            bits[bitIndex] = s[bitIndex] == '1';
        }

        return bits;
    }
}
