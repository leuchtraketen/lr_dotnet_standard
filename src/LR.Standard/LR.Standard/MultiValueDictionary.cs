﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace LR.Standard;

// Note that this is not threadsafe for concurrent reading and writing.
public interface IReadOnlyMultiValueDictionary<K, V> : IEnumerable<KeyValuePair<K, IEnumerable<V>>>, IReadOnlyDictionary<K, IEnumerable<V>> where K : notnull
{
}

public sealed class MultiValueDictionary<K, V> : IReadOnlyMultiValueDictionary<K, V> where K : notnull
{
    public struct ValueSet : IEnumerable<V?>
    {
        public struct Enumerator : IEnumerator<V?>
        {
            private readonly V? _value;
            private ImmutableHashSet<V>.Enumerator _values;
            private int _count;

            public Enumerator(ValueSet v)
            {
                if (v._value == null)
                {
                    _value = default;
                    _values = default;
                    _count = 0;
                }
                else
                {
                    ImmutableHashSet<V>? set = v._value as ImmutableHashSet<V>;
                    if (set == null)
                    {
                        _value = (V)v._value;
                        _values = default;
                        _count = 1;
                    }
                    else
                    {
                        _value = default;
                        _values = set.GetEnumerator();
                        _count = set.Count;
                        Debug.Assert(_count > 1);
                    }

                    Debug.Assert(_count == v.Count);
                }
            }

            public void Dispose()
            {
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            object? IEnumerator.Current => this.Current;

            // Note that this property is not guaranteed to throw either before MoveNext()
            // has been called or after the end of the set has been reached.
            public V? Current
            {
                get { return _count > 1 ? _values.Current : _value; }
            }

            public bool MoveNext()
            {
                switch (_count)
                {
                    case 0:
                        return false;

                    case 1:
                        _count = 0;
                        return true;

                    default:
                        if (_values.MoveNext())
                        {
                            return true;
                        }

                        _count = 0;
                        return false;
                }
            }
        }

        // Stores either a single V or an ImmutableHashSet<V>
        private readonly object? _value;

        private readonly IEqualityComparer<V> _equalityComparer;

        public int Count
        {
            get
            {
                if (_value == null)
                {
                    return 0;
                }

                // The following code used to be written like so:
                //
                //    return (_value as ImmutableHashSet<V>)?.Count ?? 1;
                //
                // This code pattern triggered a code-gen bug on Mac:
                // https://github.com/dotnet/coreclr/issues/4801

                ImmutableHashSet<V>? set = _value as ImmutableHashSet<V>;
                if (set == null)
                {
                    return 1;
                }

                return set.Count;
            }
        }

        public ValueSet(object? value, IEqualityComparer<V>? equalityComparer = null)
        {
            _value = value;
            _equalityComparer = equalityComparer ?? ImmutableHashSet<V>.Empty.KeyComparer;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<V?> IEnumerable<V?>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public ValueSet Add(V v)
        {
            Debug.Assert(_value != null);

            ImmutableHashSet<V>? set = _value as ImmutableHashSet<V>;
            if (set == null)
            {
                if (_equalityComparer.Equals((V)_value, v))
                {
                    return this;
                }

                set = ImmutableHashSet.Create(_equalityComparer, (V)_value);
            }

            return new ValueSet(set.Add(v), _equalityComparer);
        }

        public bool Contains(V v)
        {
            ImmutableHashSet<V>? set = _value as ImmutableHashSet<V>;
            if (set == null)
            {
                return _equalityComparer.Equals((V)_value, v);
            }

            return set.Contains(v);
        }

        public bool Contains(V v, IEqualityComparer<V> comparer)
        {
            foreach (V other in this)
            {
                if (comparer.Equals(other, v))
                {
                    return true;
                }
            }

            return false;
        }

        public V Single()
        {
            Debug.Assert(_value is V); // Implies value != null
            return (V)_value;
        }

        public bool Equals(ValueSet other)
        {
            return _value == other._value;
        }
    }

    private readonly Dictionary<K, ValueSet> _dictionary;

    private readonly IEqualityComparer<V>? _valueComparer;

    public int Count => _dictionary.Count;

    public bool IsEmpty => _dictionary.Count == 0;

    public Dictionary<K, ValueSet>.KeyCollection Keys => _dictionary.Keys;

    public Dictionary<K, ValueSet>.ValueCollection Values => _dictionary.Values;

    IEnumerable<K> IReadOnlyDictionary<K, IEnumerable<V>>.Keys => Keys;

    IEnumerable<IEnumerable<V>> IReadOnlyDictionary<K, IEnumerable<V>>.Values => Values.Select(o => (IEnumerable<V>)o);

    private readonly ValueSet _emptySet = new(null, null);

    // Returns an empty set if there is no such key in the dictionary.
    public IEnumerable<V> this[K k]
    {
        get { return _dictionary.TryGetValue(k, out MultiValueDictionary<K, V>.ValueSet set) ? set : _emptySet; }
    }

    public MultiValueDictionary()
    {
        _dictionary = new Dictionary<K, ValueSet>();
    }

    public MultiValueDictionary(IEqualityComparer<K> comparer)
    {
        _dictionary = new Dictionary<K, ValueSet>(comparer);
    }

    public MultiValueDictionary(int capacity, IEqualityComparer<K> comparer, IEqualityComparer<V>? valueComparer = null)
    {
        _dictionary = new Dictionary<K, ValueSet>(capacity, comparer);
        _valueComparer = valueComparer;
    }

    public bool Add(K k, V v)
    {
        ValueSet updated;

        if (_dictionary.TryGetValue(k, out ValueSet set))
        {
            updated = set.Add(v);
            if (updated.Equals(set))
            {
                return false;
            }
        }
        else
        {
            updated = new ValueSet(v, _valueComparer);
        }

        _dictionary[k] = updated;
        return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<K, IEnumerable<V>>> GetEnumerator()
    {
        return _dictionary.Select(o => new KeyValuePair<K, IEnumerable<V>>(o.Key, o.Value)).GetEnumerator();
    }

    IEnumerator<KeyValuePair<K, IEnumerable<V>>> IEnumerable<KeyValuePair<K, IEnumerable<V>>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(K k)
    {
        return _dictionary.ContainsKey(k);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public void Remove(K key)
    {
        _dictionary.Remove(key);
    }

    public bool TryGetValue(K key, out IEnumerable<V> out_value)
    {
        if (_dictionary.TryGetValue(key, out ValueSet value))
        {
            out_value = value;
            return true;
        }

        out_value = Array.Empty<V>();
        return false;
    }
}
