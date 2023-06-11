using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace LR.Standard;

public static class NumericExtensions
{
    public static double Clamp(this double self, double min, double max)
    {
        return System.Math.Min(max, System.Math.Max(self, min));
    }

    public static int Clamp(this int self, int min, int max)
    {
        return System.Math.Min(max, System.Math.Max(self, min));
    }

    public static long Clamp(this long self, long min, long max)
    {
        return System.Math.Min(max, System.Math.Max(self, min));
    }

    private static string? _resolveConstants(string? str)
    {
        if (str == null) return null;
        if (str == "true") str = "1";
        if (str == "false") str = "0";
        if (str == "null") str = null;
        return str;
    }

    public static int ToInteger(this string? str, int default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        if (int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static long ToLong(this string? str, long default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static uint ToIntegerUnsigned(this string? str, uint default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        if (uint.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out uint result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static ulong ToLongUnsigned(this string? str, ulong default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        if (ulong.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static float ToFloat(this string? str, float default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        str = ( str ?? string.Empty ).Replace(",", ".");
        if (float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static double ToDouble(this string? str, double default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        str = ( str ?? string.Empty ).Replace(",", ".");
        if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static decimal ToDecimal(this string? str, decimal default_value = 0)
    {
        str = _resolveConstants(str);
        if (string.IsNullOrWhiteSpace(str))
        {
            return default_value;
        }

        str = ( str ?? string.Empty ).Replace(",", ".");
        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            return result;
        }
        else
        {
            return default_value;
        }
    }

    public static string? IfEmpty(this string? str, string? alternative)
    {
        return string.IsNullOrEmpty(str) ? alternative : str;
    }

    public static string? IfZero<T>(this T id, string? alternative)
    {
        return ( id == null || string.IsNullOrWhiteSpace(id.ToString()) || id.ToString() == "0" ) ? alternative : id.ToString();
    }

    public static string? IfZero(this string? str, string? alternative)
    {
        return ( str == null || string.IsNullOrWhiteSpace(str) || str == "0" ) ? alternative : str;
    }

    public static T? IfDefault<T>(this T? o, T? alternative) where T : struct
    {
        if (!o.HasValue)
        {
            return alternative;
        }
        else if (o.Value.Equals(default(T)))
        {
            return alternative;
        }
        else
        {
            return o.Value;
        }
    }

    public static bool IsNull([NotNullWhen(false)] this long? num)
    {
        return num is null || num == 0;
    }

    public static bool IsNull(this long num)
    {
        return num == 0;
    }


    // byte mask = (byte)(1 << bitInByteIndex);
    // bool isSet = (bytes [byteIndex] & mask) != 0;
    // set to 1
    // bytes [byteIndex] |= mask;
    // Set to zero
    // bytes [byteIndex] &= ~mask;
    // Toggle
    // bytes [byteIndex] ^= mask;

    public static byte[] ToBinaryArray(this string s)
    {
        int length = s.Length;
        byte[] bytes = new byte [(int)System.Math.Ceiling((double)length / 8.0)];
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

    public static byte[] ToBinaryArray(this bool[] bits)
    {
        int length = bits.Length;
        byte[] bytes = new byte [(int)System.Math.Ceiling((double)length / 8.0)];
        int byteIndex, bitInByteIndex;
        byte mask;
        for (int bitIndex = 0; bitIndex < length; bitIndex++)
        {
            byteIndex = bitIndex / 8;
            bitInByteIndex = bitIndex % 8;
            mask = (byte)( 1 << bitInByteIndex );
            if (bits[bitIndex] == true)
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

    public static string ToBinaryString(this byte[] bytes)
    {
        StringBuilder sb = new();
        int length = bytes.Length;
        byte mask;
        for (int byteIndex = 0; byteIndex < length; byteIndex++)
        {
            for (int bitInByteIndex = 0; bitInByteIndex < 8; bitInByteIndex++)
            {
                mask = (byte)( 1 << bitInByteIndex );
                bool isSet = ( bytes[byteIndex] & mask ) != 0;
                sb.Append(isSet ? '1' : '0');
            }
        }

        return sb.ToString();
    }

    public static string ToBinaryString(this bool[] bits)
    {
        StringBuilder sb = new();
        int length = bits.Length;
        for (int bitIndex = 0; bitIndex < length; bitIndex++)
        {
            sb.Append(bits[bitIndex] ? '1' : '0');
        }

        return sb.ToString();
    }

    public static bool[] ToBitArray(this byte[] bytes)
    {
        int length = bytes.Length;
        bool[] bits = new bool [length * 8];
        byte mask;
        for (int byteIndex = 0; byteIndex < length; byteIndex++)
        {
            for (int bitInByteIndex = 0; bitInByteIndex < 8; bitInByteIndex++)
            {
                mask = (byte)( 1 << bitInByteIndex );
                bool isSet = ( bytes[byteIndex] & mask ) != 0;
                int bitIndex = byteIndex * 8 + bitInByteIndex;
                bits[bitIndex] = isSet;
            }
        }

        return bits;
    }

    public static bool[] ToBitArray(this string s)
    {
        int length = s.Length;
        bool[] bits = new bool [length];
        for (int bitIndex = 0; bitIndex < length; bitIndex++)
        {
            bits[bitIndex] = s[bitIndex] == '1';
        }

        return bits;
    }

    public static string ToHexString(this byte[] value)
    {
        if (value == null || value.Length == 0)
            return string.Empty;
        SoapHexBinary shb = new(value);
        return shb.ToString();
    }

    public static byte[] FromHexString(this string value)
    {
        if (value == null || value.Length == 0)
            return new byte [0];

        byte[] ret = new byte [(int)System.Math.Ceiling((double)value.Length / 2.0)];
        for (int i = 0; i * 2 < value.Length && i < ret.Length; i++)
        {
            ret[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
        }

        return ret;
    }

    public static void FromHexString(this string value, byte[] ret)
    {
        if (value == null || value.Length == 0 || ret == null)
            return;

        for (int i = 0; i * 2 < value.Length && i < ret.Length; i++)
        {
            ret[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
        }
    }
}
