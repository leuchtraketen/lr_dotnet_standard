using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LR.Standard;

public sealed class AtomicDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue> where TKey : notnull
{
    private ImmutableDictionary<TKey, TValue> _dict = ImmutableDictionary<TKey, TValue>.Empty;

    public AtomicDictionary()
    {
    }

    public TValue this[TKey key]
    {
        get => _dict[key];
        set => _dict = _dict.SetItem(key, value);
    }

    public IEnumerable<TKey> Keys => _dict.Keys;

    public IEnumerable<TValue> Values => _dict.Values;

    public ImmutableDictionary<TKey, TValue> ImmutableDictionary => _dict;

    public int Count => _dict.Count;

    public void Add(TKey key, TValue value)
    {
        ImmutableInterlocked.AddOrUpdate(ref _dict, key, value, (k, v) => value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ImmutableInterlocked.AddOrUpdate(ref _dict, item.Key, item.Value, (k, v) => item.Value);
    }

    public void Add(TKey key, [AllowNull] Func<TValue> value)
    {
        TValue value_created = default!;
        bool is_value_created = false;

        TValue value_dont_create_duplicates(TKey k)
        {
            if (!is_value_created)
            {
                is_value_created = true;
                value_created = value != null ? value.Invoke() : default!;
            }

            return value_created;
        }

        ImmutableInterlocked.AddOrUpdate(ref _dict, key, k => value_dont_create_duplicates(k), (k, v) => value_dont_create_duplicates(k));
    }

    public bool TryAdd(TKey key, [AllowNull] Func<TValue> value)
    {
        TValue value_created = default!;
        bool is_value_created = false;

        TValue value_dont_create_duplicates(TKey k)
        {
            if (!is_value_created)
            {
                is_value_created = true;
                value_created = value != null ? value.Invoke() : default!;
            }

            return value_created;
        }

        try
        {
            ImmutableInterlocked.AddOrUpdate(ref _dict, key, k => value_dont_create_duplicates(k), (k, v) => throw new TryAddPreventUpdateException());
            return true;
        }
        catch (TryAddPreventUpdateException)
        {
            return false;
        }
    }

    internal sealed class TryAddPreventUpdateException : Exception
    {
    }

    public void Clear()
    {
        _dict = _dict.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dict.Contains(item);
    }

    public bool ContainsKey(TKey key)
    {
        return _dict.ContainsKey(key);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _dict.GetEnumerator();
    }

    public void Remove(TKey key)
    {
        ImmutableInterlocked.TryRemove(ref _dict, key, out TValue value);
    }

    public void RemoveWhere(Predicate<TKey> predicate)
    {
        foreach (TKey key in _dict.Keys)
        {
            if (predicate(key))
            {
                ImmutableInterlocked.TryRemove(ref _dict, key, out TValue value);
            }
        }
    }

    public void RemoveWhere(Func<TKey, TValue, bool> predicate)
    {
        foreach (KeyValuePair<TKey, TValue> kvp in _dict)
        {
            if (predicate(kvp.Key, kvp.Value))
            {
                ImmutableInterlocked.TryRemove(ref _dict, kvp.Key, out TValue value);
            }
        }
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dict.TryGetValue(key, out value);
    }

    [return: MaybeNull]
    public TValue GetValueOrDefault(TKey key, [AllowNull] TValue default_value)
    {
        return _dict.TryGetValue(key, out TValue v) ? v : default_value;
    }

    [return: MaybeNull]
    public TValue GetValueOrDefault(TKey key, Func<TValue> default_value)
    {
        return _dict.TryGetValue(key, out TValue v) ? v : ( default_value is not null ? default_value.Invoke() : throw new ArgumentNullException(nameof(default_value)) );
    }

    [return: MaybeNull]
    public TValue GetOrAddValue(TKey key, Func<TValue> default_value)
    {
        TValue value_created = default!;
        bool is_value_created = false;

        TValue default_value_dont_create_duplicates(TKey k)
        {
            if (!is_value_created)
            {
                is_value_created = true;
                value_created = default_value is not null ? default_value.Invoke() : throw new ArgumentNullException(nameof(default_value));
            }

            return value_created;
        }

        return ImmutableInterlocked.GetOrAdd(ref _dict, key, k => default_value_dont_create_duplicates(k));
    }

    public TValue GetOrAddValue(TKey key, Func<TKey, TValue> default_value)
    {
        TValue value_created = default!;
        bool is_value_created = false;

        TValue default_value_dont_create_duplicates(TKey k)
        {
            if (!is_value_created)
            {
                is_value_created = true;
                value_created = default_value is not null ? default_value.Invoke(k) : throw new ArgumentNullException(nameof(default_value));
            }

            return value_created;
        }

        return ImmutableInterlocked.GetOrAdd(ref _dict, key, k => default_value_dont_create_duplicates(k));
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _dict.GetEnumerator();
    }
}
