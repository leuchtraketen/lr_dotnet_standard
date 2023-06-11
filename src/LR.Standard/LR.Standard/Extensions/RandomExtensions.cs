using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace LR.Standard;

public enum RandomGeneratorMode
{
    FAST = 0,
    CRYPTO = 1,
}

public static class RandomGenerator
{
    // system random generator - fast but not very random
    private static readonly Random rand = new();

    // crypto random generator
    private static readonly RNGCryptoServiceProvider cryptoProvider = new();

    [ThreadStatic] private static readonly byte[] buf_uint = new byte [sizeof(uint)];
    [ThreadStatic] private static readonly byte[] buf_ulong = new byte [sizeof(ulong)];

    public static double NextDouble(RandomGeneratorMode mode)
    {
        switch (mode)
        {
            case RandomGeneratorMode.CRYPTO:
                cryptoProvider.GetBytes(buf_uint);
                double d = BitConverter.ToUInt32(buf_uint, 0) / (double)uint.MaxValue;
                return d;

            case RandomGeneratorMode.FAST:
            default:
                return rand.NextDouble();
        }
    }

    public static ulong NextUnsignedInt(RandomGeneratorMode mode)
    {
        switch (mode)
        {
            case RandomGeneratorMode.CRYPTO:
                cryptoProvider.GetBytes(buf_uint);
                ulong d = BitConverter.ToUInt32(buf_uint, 0);
                return d;

            case RandomGeneratorMode.FAST:
            default:
                return (ulong)rand.NextLong();
        }
    }

    public static ulong NextUnsignedLong(RandomGeneratorMode mode)
    {
        switch (mode)
        {
            case RandomGeneratorMode.CRYPTO:
                cryptoProvider.GetBytes(buf_ulong);
                ulong d = BitConverter.ToUInt64(buf_ulong, 0);
                return d;

            case RandomGeneratorMode.FAST:
            default:
                return (ulong)rand.NextLong();
        }
    }

    public static int Next(RandomGeneratorMode mode, int inclusiveLowerBound, int exclusiveUpperBound)
    {
        switch (mode)
        {
            case RandomGeneratorMode.CRYPTO:
                cryptoProvider.GetBytes(buf_uint);
                return inclusiveLowerBound + (int)( BitConverter.ToUInt32(buf_uint, 0) % ( exclusiveUpperBound - inclusiveLowerBound ) );

            case RandomGeneratorMode.FAST:
            default:
                return rand.Next(exclusiveUpperBound);
        }
    }
}

public static class RandomExtensions
{
    public static void ShuffleInPlace<T>(this T[] array, RandomGeneratorMode mode = default)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = RandomGenerator.Next(mode, 0, n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

    public static T[] Shuffle<T>(this IEnumerable<T> enumerable, RandomGeneratorMode mode = default)
    {
        T[] array = enumerable.ToArray();
        int n = array.Length;
        while (n > 1)
        {
            int k = RandomGenerator.Next(mode, 0, n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }

        return array;
    }

    /// <summary>
    /// Returns a random long from min (inclusive) to max (exclusive)
    /// </summary>
    /// <param name="random">The given random instance</param>
    /// <param name="min">The inclusive minimum bound</param>
    /// <param name="max">The exclusive maximum bound.  Must be greater than min</param>
    public static long NextLong(this Random random, long min, long max)
    {
        if (max <= min)
            throw new ArgumentOutOfRangeException("max", "max must be > min!");

        //Working with ulong so that modulo works correctly with values > long.MaxValue
        ulong uRange = (ulong)( max - min );

        //Prevent a modolo bias; see http://stackoverflow.com/a/10984975/238419
        //for more information.
        //In the worst case, the expected number of calls is 2 (though usually it's
        //much closer to 1) so this loop doesn't really hurt performance at all.
        ulong ulongRand;
        do
        {
            byte[] buf = new byte [8];
            random.NextBytes(buf);
            ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
        } while (ulongRand > ulong.MaxValue - ( ( ulong.MaxValue % uRange ) + 1 ) % uRange);

        return (long)( ulongRand % uRange ) + min;
    }

    /// <summary>
    /// Returns a random long from 0 (inclusive) to max (exclusive)
    /// </summary>
    /// <param name="random">The given random instance</param>
    /// <param name="max">The exclusive maximum bound.  Must be greater than 0</param>
    public static long NextLong(this Random random, long max)
    {
        return random.NextLong(0, max);
    }

    /// <summary>
    /// Returns a random long over all possible values of long (except long.MaxValue, similar to
    /// random.Next())
    /// </summary>
    /// <param name="random">The given random instance</param>
    public static long NextLong(this Random random)
    {
        return random.NextLong(long.MinValue, long.MaxValue);
    }
}
