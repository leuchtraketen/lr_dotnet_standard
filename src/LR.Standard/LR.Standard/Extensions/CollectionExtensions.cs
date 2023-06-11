using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LR.Standard;

public static class StandardCollectionExtensions
{
    public static T Get<T>(this IReadOnlyList<T> list, int index, T default_value)
    {
        return index >= 0 && index < list.Count ? list[index] : default_value;
    }

    public static void Set<T>(this IList<T> list, int index, T value)
    {
        if (index >= 0 && index < list.Count)
        {
            list[index] = value;
        }
    }

    public static V Get<K, V>(this IReadOnlyDictionary<K, V> dict, K key, V default_value) where K : notnull
    {
        return dict.TryGetValue(key, out V res) ? res : default_value;
    }

    public static ImmutableDictionary<K, V> ToImmutableDictionarySafe<T, K, V>(this IEnumerable<T> enumerable, Func<T, K> key, Func<T, V> value) where K : notnull
    {
        ImmutableDictionary<K, V> result = ImmutableDictionary<K, V>.Empty;
        foreach (T item in enumerable)
        {
            result = result.SetItem(key(item), value(item));
        }

        return result;
    }

    public static ConcurrentDictionary<K, V> ToConcurrentDictionarySafe<T, K, V>(this IEnumerable<T> enumerable, Func<T, K> key, Func<T, V> value) where K : notnull
    {
        ConcurrentDictionary<K, V> result = new();
        foreach (T item in enumerable)
        {
            result[key(item)] = value(item);
        }

        return result;
    }

    public static Dictionary<K, V> ToDictionarySafe<T, K, V>(this IEnumerable<T> enumerable, Func<T, K> key, Func<T, V> value) where K : notnull
    {
        Dictionary<K, V> result = new();
        foreach (T item in enumerable)
        {
            result[key(item)] = value(item);
        }

        return result;
    }

    public static MultiValueDictionary<K, V> ToMultiValueDictionarySafe<T, K, V>(this IEnumerable<T> enumerable, Func<T, K> key, Func<T, V> value)
    {
        MultiValueDictionary<K, V> result = new();
        foreach (T item in enumerable)
        {
            result.Add(key(item), value(item));
        }

        return result;
    }

    public static async Task<Dictionary<K, V>> ToDictionarySafeAsync<T, K, V>(this IEnumerable<T> enumerable, Func<T, ValueTask<K>> key, Func<T, ValueTask<V>> value)
        where K : notnull
    {
        Dictionary<K, V> result = new();
        foreach (T item in enumerable)
        {
            result[await key(item)] = await value(item);
        }

        return result;
    }

    public static async Task<MultiValueDictionary<K, V>> ToMultiValueDictionarySafeAsync<T, K, V>(this IEnumerable<T> enumerable, Func<T, ValueTask<K>> key,
        Func<T, ValueTask<V>> value)
    {
        MultiValueDictionary<K, V> result = new();
        foreach (T item in enumerable)
        {
            result.Add(await key(item), await value(item));
        }

        return result;
    }

    /*
    public static IEnumerable<T> TakeLast<T> (this IEnumerable<T> source, int N)
    {
        T [] enumerable = source as T [] ?? source.ToArray ();
        return enumerable.Skip (System.Math.Max (0, enumerable.Count () - N));
    }

    public static IEnumerable<T> SkipLast<T> (this IEnumerable<T> source, int N)
    {
        T [] enumerable = source as T [] ?? source.ToArray ();
        return enumerable.Take (System.Math.Max (0, enumerable.Count () - N));
    }
    */

    public static string Join<T>(this IEnumerable<T> enumerable, string delimiter)
    {
        // return string.Join (delimiter, enumerable.Select (e => e?.ToString () ?? string.Empty).ToArray ());
        StringBuilder sb = new();
        string sep = "";
        foreach (T item in enumerable)
        {
            sb.Append(sep);
            sb.Append(item?.ToString() ?? string.Empty);
            sep = delimiter;
        }

        return sb.ToString();
    }

    public static string Join(this string[] enumerable, string delimiter)
    {
        return string.Join(delimiter, enumerable);
    }

    public static string Join(this IReadOnlyList<string?> enumerable, string delimiter)
    {
        return string.Join(delimiter, enumerable);
    }

    public static string Join(this IEnumerable<string?> enumerable, string delimiter)
    {
        return string.Join(delimiter, enumerable);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static T[] Extend<T>(this T[] firstArray, params T[] secondArray) where T : class
    {
        if (secondArray == null)
        {
            throw new ArgumentNullException("secondArray");
        }

        if (firstArray == null)
        {
            return secondArray;
        }

        return firstArray.Concat(secondArray).ToArray(); // although Concat is not recommended for performance reasons
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IEnumerable<T> Extend<T>(this IEnumerable<T> firstArray, params T[] secondArray)
    {
        if (secondArray == null)
        {
            throw new ArgumentNullException("secondArray");
        }

        if (firstArray == null)
        {
            return secondArray;
        }

        return firstArray.Concat(secondArray);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static ImmutableArray<T> Extend<T>(this ImmutableArray<T> firstImmutableArray, params T[] secondArray)
    {
        if (secondArray == null)
        {
            throw new ArgumentNullException("secondArray");
        }

        if (firstImmutableArray.IsDefaultOrEmpty)
        {
            return secondArray.ToImmutableArray();
        }

        return firstImmutableArray.Concat(secondArray).ToImmutableArray();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static T[] Extend<T>(this T firstItem, params T[] secondArray) where T : class
    {
        if (secondArray == null)
        {
            throw new ArgumentNullException("secondArray");
        }

        if (firstItem == null)
        {
            return secondArray;
        }

        return new T[] { firstItem }.Concat(secondArray).ToArray(); // although Concat is not recommended for performance reasons
    }

    public static ImmutableArray<T> EmptyIfDefault<T>(this ImmutableArray<T> array)
    {
        if (array.IsDefault)
            return ImmutableArray<T>.Empty;
        return array;
    }

    public static byte[] ToByteArray(this Stream stream)
    {
        if (stream is MemoryStream mem_stream)
        {
            return mem_stream.ToArray();
        }
        else
        {
            stream.Position = 0;
            using MemoryStream mem_stream2 = new();
            stream.CopyTo(mem_stream2);
            return mem_stream2.ToArray();
        }
    }

    public static IEnumerable<List<T>> Partition<T>(this IEnumerable<T> source, int size)
    {
        int count;
        if (source is IReadOnlyCollection<T> source_readonlycollection)
        {
            count = source_readonlycollection.Count;
        }
        else if (source is ICollection<T> source_collection)
        {
            count = source_collection.Count;
        }
        else
        {
            List<T> source_list = source.ToList();
            count = source_list.Count;
            source = source_list;
        }

        for (int i = 0; i < count; i += size)
        {
            yield return new List<T>(source.Skip(i).Take(size));
        }
    }

    public static bool IsAny<T>(this T self, params T[] choices)
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAny<T>(this T? self, params T[] choices) where T : struct
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNone<T>(this T self, params T[] choices)
    {
        return !IsAny<T>(self, choices);
    }

    public static bool IsNone<T>(this T? self, params T[] choices) where T : struct
    {
        return !IsAny<T>(self, choices);
    }

    public static bool IsAny<T>(this T self, IReadOnlyList<T> choices)
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAny<T>(this T? self, IReadOnlyList<T> choices) where T : struct
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNone<T>(this T self, IReadOnlyList<T> choices)
    {
        return !IsAny<T>(self, choices);
    }

    public static bool IsNone<T>(this T? self, IReadOnlyList<T> choices) where T : struct
    {
        return !IsAny<T>(self, choices);
    }

    public static bool IsAny<T>(this T self, IEnumerable<T> choices)
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAny<T>(this T? self, IEnumerable<T> choices) where T : struct
    {
        if (self == null)
            return false;
        foreach (T choice in choices)
        {
            if (self.Equals(choice))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNone<T>(this T self, IEnumerable<T> choices)
    {
        return !IsAny<T>(self, choices);
    }

    public static bool IsNone<T>(this T? self, IEnumerable<T> choices) where T : struct
    {
        return !IsAny<T>(self, choices);
    }

    public static bool AddRange<T>(this HashSet<T> self, IEnumerable<T> items)
    {
        bool allAdded = true;
        foreach (T item in items)
        {
            allAdded &= self.Add(item);
        }

        return allAdded;
    }

    public static bool None<TSource>(this IEnumerable<TSource> source)
    {
        return !source.Any();
    }

    public static bool None<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        return !source.Any(predicate);
    }

    public static byte[] ToByteArray<T>(this T[] source) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            byte[] destination = new byte [source.Length * Marshal.SizeOf(typeof(T))];
            Marshal.Copy(pointer, destination, 0, destination.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }

    public static T[] FromByteArray<T>(this byte[] source) where T : struct
    {
        T[] destination = new T [source.Length / Marshal.SizeOf(typeof(T))];
        GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            IntPtr pointer = handle.AddrOfPinnedObject();
            Marshal.Copy(source, 0, pointer, source.Length);
            return destination;
        }
        finally
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }

    public static void Fill<T>(this T[] array, T value)
    {
#if NET471 || NETSTANDARD2_0
            for (int i = 0; i < array.Length; i++)
            {
                array [i] = value;
            }
#else
        Array.Fill(array, value);
#endif
    }

    public static void Fill<T>(this T[] array, T value, int index, int count)
    {
#if NET471 || NETSTANDARD2_0
            if (count <= 0)
                return;

            array [index] = value;

            int copied = 1;
            int i = index;

            while (copied < count)
            {
                Array.Copy (array, index, array, index + copied, Math.Min (copied, count - copied));
                copied += copied;
            }

            /*
            int length = Math.Min (index + count, array.Length);
            for (int i = index; i < length; i++)
            {
                array [i] = value;
            }
            */
#else
        if (count > 0)
        {
            Array.Fill(array, value, index, count);
        }
#endif
    }

    public static async IAsyncEnumerable<TSource> WhereAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
    {
        foreach (TSource value in source)
        {
            if (await predicate(value))
            {
                yield return value;
            }
        }
    }

    public static async Task<IEnumerable<TSource>> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
    {
        List<TSource> ret = new();
        foreach (TSource value in source)
        {
            if (await predicate(value))
            {
                ret.Add(value);
            }
        }

        return ret;
    }

    public static async Task<IEnumerable<TTarget>> Select<TSource, TTarget>(this IEnumerable<TSource> source, Func<TSource, ValueTask<TTarget>> func)
    {
        List<TTarget> ret = new();
        foreach (TSource value in source)
        {
            ret.Add(await func(value));
        }

        return ret;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (T? value in source)
        {
            if (!( value is null ))
            {
                yield return value;
            }
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source, Func<TSource, ValueTask<bool>> predicate)
    {
        foreach (TSource value in source)
        {
            yield return value;
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously


    public static void ReplaceWith<T>(this IList<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (T o in source)
        {
            target.Add(o);
        }
    }
}
