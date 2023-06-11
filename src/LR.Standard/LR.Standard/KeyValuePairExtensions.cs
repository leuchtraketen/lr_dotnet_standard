using System;
using System.Collections.Generic;
using System.Text;

namespace LR.Standard;

public static class KeyValuePairExtensions
{
    public static void Deconstruct<K, V>(this KeyValuePair<K, V> kvp, out K key, out V value)
    {
        key = kvp.Key;
        value = kvp.Value;
    }
}
