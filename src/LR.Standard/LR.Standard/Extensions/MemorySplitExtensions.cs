using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LR.Standard;

public static class MemorySplitExtensions
{
    public ref struct Enumerable1<T> where T : IEquatable<T>
    {
        public Enumerable1(ReadOnlyMemory<T> span, T separator)
        {
            this.Memory = span;
            this.Separator = separator;
        }

        ReadOnlyMemory<T> Memory { get; }
        T Separator { get; }

        public Enumerator1<T> GetEnumerator() => new(this.Memory, this.Separator);
    }

    public ref struct Enumerable2<T> where T : IEquatable<T>
    {
        public Enumerable2(ReadOnlyMemory<T> span, T separator1, T separator2)
        {
            this.Memory = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
        }

        ReadOnlyMemory<T> Memory { get; }
        T Separator1 { get; }
        T Separator2 { get; }

        public Enumerator2<T> GetEnumerator() => new(this.Memory, this.Separator1, this.Separator2);
    }

    public ref struct Enumerable3<T> where T : IEquatable<T>
    {
        public Enumerable3(ReadOnlyMemory<T> span, T separator1, T separator2, T separator3)
        {
            this.Memory = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Separator3 = separator3;
        }

        ReadOnlyMemory<T> Memory { get; }
        T Separator1 { get; }
        T Separator2 { get; }
        T Separator3 { get; }

        public Enumerator3<T> GetEnumerator() =>
            new(this.Memory, this.Separator1, this.Separator2, this.Separator3);
    }

    public ref struct EnumerableN<T> where T : IEquatable<T>
    {
        public EnumerableN(ReadOnlyMemory<T> span, ReadOnlySpan<T> separators)
        {
            this.Memory = span;
            this.Separators = separators;
        }

        ReadOnlyMemory<T> Memory { get; }
        ReadOnlySpan<T> Separators { get; }

        public EnumeratorN<T> GetEnumerator() => new(this.Memory, this.Separators);
    }

    public ref struct EnumerableSequence<T> where T : IEquatable<T>
    {
        public EnumerableSequence(ReadOnlyMemory<T> span, ReadOnlySpan<T> separator_sequence)
        {
            this.Span = span;
            this.SeparatorSequence = separator_sequence;
        }

        ReadOnlyMemory<T> Span { get; }
        ReadOnlySpan<T> SeparatorSequence { get; }

        public EnumeratorSequence<T> GetEnumerator() => new(this.Span, this.SeparatorSequence);
    }

    public ref struct Enumerator1<T> where T : IEquatable<T>
    {
        public Enumerator1(ReadOnlyMemory<T> span, T separator)
        {
            this.Memory = span;
            this.Separator = separator;
            this.Current = default;

            this.HasTrailingEmptyItem = this.Memory.IsEmpty;
        }

        ReadOnlyMemory<T> Memory { get; set; }
        bool HasTrailingEmptyItem { get; set; }
        T Separator { get; }
        int SeparatorLength => 1;

        public bool MoveNext()
        {
            if (this.HasTrailingEmptyItem)
            {
                this.HasTrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Memory.IsEmpty)
            {
                this.Memory = this.Current = default;
                return false;
            }

            int idx = this.Memory.Span.IndexOf(this.Separator);
            if (idx < 0)
            {
                this.Current = this.Memory;
                this.Memory = default;
            }
            else
            {
                this.Current = this.Memory.Slice(0, idx);
                this.Memory = this.Memory.Slice(idx + this.SeparatorLength);
            }

            return true;
        }

        public ReadOnlyMemory<T> Current { get; private set; }
    }

    public ref struct Enumerator2<T> where T : IEquatable<T>
    {
        public Enumerator2(ReadOnlyMemory<T> span, T separator1, T separator2)
        {
            this.Memory = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Current = default;

            this.HasTrailingEmptyItem = this.Memory.IsEmpty;
        }

        ReadOnlyMemory<T> Memory { get; set; }
        bool HasTrailingEmptyItem { get; set; }
        T Separator1 { get; }
        T Separator2 { get; }
        int SeparatorLength => 1;

        public bool MoveNext()
        {
            if (this.HasTrailingEmptyItem)
            {
                this.HasTrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Memory.IsEmpty)
            {
                this.Memory = this.Current = default;
                return false;
            }

            int idx = this.Memory.Span.IndexOfAny(this.Separator1, this.Separator2);
            if (idx < 0)
            {
                this.Current = this.Memory;
                this.Memory = default;
            }
            else
            {
                this.Current = this.Memory.Slice(0, idx);
                this.Memory = this.Memory.Slice(idx + this.SeparatorLength);
                this.HasTrailingEmptyItem = this.Memory.IsEmpty;
            }

            return true;
        }

        public ReadOnlyMemory<T> Current { get; private set; }
    }

    public ref struct Enumerator3<T> where T : IEquatable<T>
    {
        public Enumerator3(ReadOnlyMemory<T> span, T separator1, T separator2, T separator3)
        {
            this.Memory = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Separator3 = separator3;
            this.Current = default;

            this.HasTrailingEmptyItem = this.Memory.IsEmpty;
        }

        ReadOnlyMemory<T> Memory { get; set; }
        bool HasTrailingEmptyItem { get; set; }
        T Separator1 { get; }
        T Separator2 { get; }
        T Separator3 { get; }
        int SeparatorLength => 1;

        public bool MoveNext()
        {
            if (this.HasTrailingEmptyItem)
            {
                this.HasTrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Memory.IsEmpty)
            {
                this.Memory = this.Current = default;
                return false;
            }

            int idx = this.Memory.Span.IndexOfAny(this.Separator1, this.Separator2, this.Separator3);
            if (idx < 0)
            {
                this.Current = this.Memory;
                this.Memory = default;
            }
            else
            {
                this.Current = this.Memory.Slice(0, idx);
                this.Memory = this.Memory.Slice(idx + this.SeparatorLength);
                this.HasTrailingEmptyItem = this.Memory.IsEmpty;
            }

            return true;
        }

        public ReadOnlyMemory<T> Current { get; private set; }
    }

    public ref struct EnumeratorN<T> where T : IEquatable<T>
    {
        public EnumeratorN(ReadOnlyMemory<T> span, ReadOnlySpan<T> separators)
        {
            this.Memory = span;
            this.Separators = separators;
            this.Current = default;

            this.HasTrailingEmptyItem = this.Memory.IsEmpty;
        }

        ReadOnlyMemory<T> Memory { get; set; }
        bool HasTrailingEmptyItem { get; set; }
        ReadOnlySpan<T> Separators { get; }
        int SeparatorLength => 1;

        public bool MoveNext()
        {
            if (this.HasTrailingEmptyItem)
            {
                this.HasTrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Memory.IsEmpty)
            {
                this.Memory = this.Current = default;
                return false;
            }

            int idx = this.Memory.Span.IndexOfAny(this.Separators);
            if (idx < 0)
            {
                this.Current = this.Memory;
                this.Memory = default;
            }
            else
            {
                this.Current = this.Memory.Slice(0, idx);
                this.Memory = this.Memory.Slice(idx + this.SeparatorLength);
                this.HasTrailingEmptyItem = this.Memory.IsEmpty;
            }

            return true;
        }

        public ReadOnlyMemory<T> Current { get; private set; }
    }

    public ref struct EnumeratorSequence<T> where T : IEquatable<T>
    {
        public EnumeratorSequence(ReadOnlyMemory<T> span, ReadOnlySpan<T> separator_sequence)
        {
            this.Memory = span;
            this.SeparatorSequence = separator_sequence;
            this.Current = default;

            this.HasTrailingEmptyItem = this.Memory.IsEmpty;
        }

        ReadOnlyMemory<T> Memory { get; set; }
        bool HasTrailingEmptyItem { get; set; }
        ReadOnlySpan<T> SeparatorSequence { get; }

        public bool MoveNext()
        {
            if (this.HasTrailingEmptyItem)
            {
                this.HasTrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Memory.IsEmpty)
            {
                this.Memory = this.Current = default;
                return false;
            }

            int idx = this.Memory.Span.IndexOf(this.SeparatorSequence);
            if (idx < 0)
            {
                this.Current = this.Memory;
                this.Memory = default;
            }
            else
            {
                this.Current = this.Memory.Slice(0, idx);
                this.Memory = this.Memory.Slice(idx + this.SeparatorSequence.Length);
                this.HasTrailingEmptyItem = this.Memory.IsEmpty;
            }

            return true;
        }

        public ReadOnlyMemory<T> Current { get; private set; }
    }

    [Pure]
    public static Enumerable1<T> Split<T>(this ReadOnlyMemory<T> span, T separator)
        where T : IEquatable<T> => new(span, separator);

    [Pure]
    public static Enumerable2<T> SplitMultiple<T>(this ReadOnlyMemory<T> span, T separator1, T separator2)
        where T : IEquatable<T> => new(span, separator1, separator2);

    [Pure]
    public static Enumerable3<T> SplitMultiple<T>(this ReadOnlyMemory<T> span, T separator1, T separator2, T separator3)
        where T : IEquatable<T> => new(span, separator1, separator2, separator3);

    [Pure]
    public static EnumerableN<T> SplitMultiple<T>(this ReadOnlyMemory<T> span, ReadOnlySpan<T> values)
        where T : IEquatable<T> => new(span, values);

    [Pure]
    public static EnumerableSequence<T> Split<T>(this ReadOnlyMemory<T> span, ReadOnlySpan<T> values)
        where T : IEquatable<T> => new(span, values);


    public static async ValueTask SplitAsync<T>(this ReadOnlyMemory<T> data, T separator, StringSplitOptions options, Func<ReadOnlyMemory<T>, ValueTask> callback)
        where T : IEquatable<T>
    {
        bool remove_empty_entries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

        ReadOnlyMemory<T> buffer_remaining = data;
        while (buffer_remaining.Length != 0)
        {
            ReadOnlyMemory<T> piece;
            if (buffer_remaining.Span.IndexOf(separator) is int index_separator && index_separator != -1)
            {
                piece = buffer_remaining.Slice(0, index_separator);
                buffer_remaining = buffer_remaining.Slice(index_separator + 1);
            }
            else
            {
                piece = buffer_remaining;
                buffer_remaining = ReadOnlyMemory<T>.Empty;
            }

            if (remove_empty_entries && piece.Length == 0)
                continue;

            await callback(piece);
        }
    }

    public static async ValueTask SplitAsync<T>(this ReadOnlyMemory<T> data, ReadOnlyMemory<T> separator, StringSplitOptions options, Func<ReadOnlyMemory<T>, ValueTask> callback)
        where T : IEquatable<T>
    {
        bool remove_empty_entries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

        ReadOnlyMemory<T> buffer_remaining = data;
        while (buffer_remaining.Length != 0)
        {
            ReadOnlyMemory<T> piece;
            if (buffer_remaining.Span.IndexOf(separator.Span) is int index_separator && index_separator != -1)
            {
                piece = buffer_remaining.Slice(0, index_separator);
                buffer_remaining = buffer_remaining.Slice(index_separator + separator.Length);
            }
            else
            {
                piece = buffer_remaining;
                buffer_remaining = ReadOnlyMemory<T>.Empty;
            }

            if (remove_empty_entries && piece.Length == 0)
                continue;

            await callback(piece);
        }
    }

    public static void Split<T>(this ReadOnlyMemory<T> data, T separator, StringSplitOptions options, Action<ReadOnlyMemory<T>> callback) where T : IEquatable<T>
    {
        bool remove_empty_entries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

        ReadOnlyMemory<T> buffer_remaining = data;
        while (buffer_remaining.Length != 0)
        {
            ReadOnlyMemory<T> piece;
            if (buffer_remaining.Span.IndexOf(separator) is int index_separator && index_separator != -1)
            {
                piece = buffer_remaining.Slice(0, index_separator);
                buffer_remaining = buffer_remaining.Slice(index_separator + 1);
            }
            else
            {
                piece = buffer_remaining;
                buffer_remaining = ReadOnlyMemory<T>.Empty;
            }

            if (remove_empty_entries && piece.Length == 0)
                continue;

            callback(piece);
        }
    }

    public static void Split<T>(this ReadOnlyMemory<T> data, ReadOnlySpan<T> separator, StringSplitOptions options, Action<ReadOnlyMemory<T>> callback) where T : IEquatable<T>
    {
        bool remove_empty_entries = options.HasFlag(StringSplitOptions.RemoveEmptyEntries);

        ReadOnlyMemory<T> buffer_remaining = data;
        while (buffer_remaining.Length != 0)
        {
            ReadOnlyMemory<T> piece;
            if (buffer_remaining.Span.IndexOf(separator) is int index_separator && index_separator != -1)
            {
                piece = buffer_remaining.Slice(0, index_separator);
                buffer_remaining = buffer_remaining.Slice(index_separator + separator.Length);
            }
            else
            {
                piece = buffer_remaining;
                buffer_remaining = ReadOnlyMemory<T>.Empty;
            }

            if (remove_empty_entries && piece.Length == 0)
                continue;

            callback(piece);
        }
    }
}
