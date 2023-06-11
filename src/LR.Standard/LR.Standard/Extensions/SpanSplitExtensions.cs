using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LR.Standard;

public static class SpanSplitExtensions
{
    public ref struct Enumerable1<T> where T : IEquatable<T>
    {
        public Enumerable1(ReadOnlySpan<T> span, T separator)
        {
            this.Span = span;
            this.Separator = separator;
        }

        ReadOnlySpan<T> Span { get; }
        T Separator { get; }

        public Enumerator1<T> GetEnumerator() => new(this.Span, this.Separator);
    }

    public ref struct Enumerable2<T> where T : IEquatable<T>
    {
        public Enumerable2(ReadOnlySpan<T> span, T separator1, T separator2)
        {
            this.Span = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
        }

        ReadOnlySpan<T> Span { get; }
        T Separator1 { get; }
        T Separator2 { get; }

        public Enumerator2<T> GetEnumerator() => new(this.Span, this.Separator1, this.Separator2);
    }

    public ref struct Enumerable3<T> where T : IEquatable<T>
    {
        public Enumerable3(ReadOnlySpan<T> span, T separator1, T separator2, T separator3)
        {
            this.Span = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Separator3 = separator3;
        }

        ReadOnlySpan<T> Span { get; }
        T Separator1 { get; }
        T Separator2 { get; }
        T Separator3 { get; }

        public Enumerator3<T> GetEnumerator() =>
            new(this.Span, this.Separator1, this.Separator2, this.Separator3);
    }

    public ref struct EnumerableN<T> where T : IEquatable<T>
    {
        public EnumerableN(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
        {
            this.Span = span;
            this.Separators = separators;
        }

        ReadOnlySpan<T> Span { get; }
        ReadOnlySpan<T> Separators { get; }

        public EnumeratorN<T> GetEnumerator() => new(this.Span, this.Separators);
    }

    public ref struct EnumerableSequence<T> where T : IEquatable<T>
    {
        public EnumerableSequence(ReadOnlySpan<T> span, ReadOnlySpan<T> separator_sequence)
        {
            this.Span = span;
            this.SeparatorSequence = separator_sequence;
        }

        ReadOnlySpan<T> Span { get; }
        ReadOnlySpan<T> SeparatorSequence { get; }

        public EnumeratorSequence<T> GetEnumerator() => new(this.Span, this.SeparatorSequence);
    }

    public ref struct Enumerator1<T> where T : IEquatable<T>
    {
        public Enumerator1(ReadOnlySpan<T> span, T separator)
        {
            this.Span = span;
            this.Separator = separator;
            this.Current = default;

            if (this.Span.IsEmpty)
                this.TrailingEmptyItem = true;
        }

        ReadOnlySpan<T> Span { get; set; }
        T Separator { get; }
        int SeparatorLength => 1;

        ReadOnlySpan<T> TrailingEmptyItemSentinel => Unsafe.As<T[]>(nameof(this.TrailingEmptyItemSentinel)).AsSpan();

        bool TrailingEmptyItem
        {
            get => this.Span == this.TrailingEmptyItemSentinel;
            set => this.Span = value ? this.TrailingEmptyItemSentinel : default;
        }

        public bool MoveNext()
        {
            if (this.TrailingEmptyItem)
            {
                this.TrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Span.IsEmpty)
            {
                this.Span = this.Current = default;
                return false;
            }

            int idx = this.Span.IndexOf(this.Separator);
            if (idx < 0)
            {
                this.Current = this.Span;
                this.Span = default;
            }
            else
            {
                this.Current = this.Span.Slice(0, idx);
                this.Span = this.Span.Slice(idx + this.SeparatorLength);
                if (this.Span.IsEmpty)
                    this.TrailingEmptyItem = true;
            }

            return true;
        }

        public ReadOnlySpan<T> Current { get; private set; }
    }

    public ref struct Enumerator2<T> where T : IEquatable<T>
    {
        public Enumerator2(ReadOnlySpan<T> span, T separator1, T separator2)
        {
            this.Span = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Current = default;

            if (this.Span.IsEmpty)
                this.TrailingEmptyItem = true;
        }

        ReadOnlySpan<T> Span { get; set; }
        T Separator1 { get; }
        T Separator2 { get; }
        int SeparatorLength => 1;

        ReadOnlySpan<T> TrailingEmptyItemSentinel => Unsafe.As<T[]>(nameof(this.TrailingEmptyItemSentinel)).AsSpan();

        bool TrailingEmptyItem
        {
            get => this.Span == this.TrailingEmptyItemSentinel;
            set => this.Span = value ? this.TrailingEmptyItemSentinel : default;
        }

        public bool MoveNext()
        {
            if (this.TrailingEmptyItem)
            {
                this.TrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Span.IsEmpty)
            {
                this.Span = this.Current = default;
                return false;
            }

            int idx = this.Span.IndexOfAny(this.Separator1, this.Separator2);
            if (idx < 0)
            {
                this.Current = this.Span;
                this.Span = default;
            }
            else
            {
                this.Current = this.Span.Slice(0, idx);
                this.Span = this.Span.Slice(idx + this.SeparatorLength);
                if (this.Span.IsEmpty)
                    this.TrailingEmptyItem = true;
            }

            return true;
        }

        public ReadOnlySpan<T> Current { get; private set; }
    }

    public ref struct Enumerator3<T> where T : IEquatable<T>
    {
        public Enumerator3(ReadOnlySpan<T> span, T separator1, T separator2, T separator3)
        {
            this.Span = span;
            this.Separator1 = separator1;
            this.Separator2 = separator2;
            this.Separator3 = separator3;
            this.Current = default;

            if (this.Span.IsEmpty)
                this.TrailingEmptyItem = true;
        }

        ReadOnlySpan<T> Span { get; set; }
        T Separator1 { get; }
        T Separator2 { get; }
        T Separator3 { get; }
        int SeparatorLength => 1;

        ReadOnlySpan<T> TrailingEmptyItemSentinel => Unsafe.As<T[]>(nameof(this.TrailingEmptyItemSentinel)).AsSpan();

        bool TrailingEmptyItem
        {
            get => this.Span == this.TrailingEmptyItemSentinel;
            set => this.Span = value ? this.TrailingEmptyItemSentinel : default;
        }

        public bool MoveNext()
        {
            if (this.TrailingEmptyItem)
            {
                this.TrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Span.IsEmpty)
            {
                this.Span = this.Current = default;
                return false;
            }

            int idx = this.Span.IndexOfAny(this.Separator1, this.Separator2, this.Separator3);
            if (idx < 0)
            {
                this.Current = this.Span;
                this.Span = default;
            }
            else
            {
                this.Current = this.Span.Slice(0, idx);
                this.Span = this.Span.Slice(idx + this.SeparatorLength);
                if (this.Span.IsEmpty)
                    this.TrailingEmptyItem = true;
            }

            return true;
        }

        public ReadOnlySpan<T> Current { get; private set; }
    }

    public ref struct EnumeratorN<T> where T : IEquatable<T>
    {
        public EnumeratorN(ReadOnlySpan<T> span, ReadOnlySpan<T> separators)
        {
            this.Span = span;
            this.Separators = separators;
            this.Current = default;

            if (this.Span.IsEmpty)
                this.TrailingEmptyItem = true;
        }

        ReadOnlySpan<T> Span { get; set; }
        ReadOnlySpan<T> Separators { get; }
        int SeparatorLength => 1;

        ReadOnlySpan<T> TrailingEmptyItemSentinel => Unsafe.As<T[]>(nameof(this.TrailingEmptyItemSentinel)).AsSpan();

        bool TrailingEmptyItem
        {
            get => this.Span == this.TrailingEmptyItemSentinel;
            set => this.Span = value ? this.TrailingEmptyItemSentinel : default;
        }

        public bool MoveNext()
        {
            if (this.TrailingEmptyItem)
            {
                this.TrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Span.IsEmpty)
            {
                this.Span = this.Current = default;
                return false;
            }

            int idx = this.Span.IndexOfAny(this.Separators);
            if (idx < 0)
            {
                this.Current = this.Span;
                this.Span = default;
            }
            else
            {
                this.Current = this.Span.Slice(0, idx);
                this.Span = this.Span.Slice(idx + this.SeparatorLength);
                if (this.Span.IsEmpty)
                    this.TrailingEmptyItem = true;
            }

            return true;
        }

        public ReadOnlySpan<T> Current { get; private set; }
    }

    public ref struct EnumeratorSequence<T> where T : IEquatable<T>
    {
        public EnumeratorSequence(ReadOnlySpan<T> span, ReadOnlySpan<T> separator_sequence)
        {
            this.Span = span;
            this.SeparatorSequence = separator_sequence;
            this.Current = default;

            if (this.Span.IsEmpty)
                this.TrailingEmptyItem = true;
        }

        ReadOnlySpan<T> Span { get; set; }
        ReadOnlySpan<T> SeparatorSequence { get; }

        ReadOnlySpan<T> TrailingEmptyItemSentinel => Unsafe.As<T[]>(nameof(this.TrailingEmptyItemSentinel)).AsSpan();

        bool TrailingEmptyItem
        {
            get => this.Span == this.TrailingEmptyItemSentinel;
            set => this.Span = value ? this.TrailingEmptyItemSentinel : default;
        }

        public bool MoveNext()
        {
            if (this.TrailingEmptyItem)
            {
                this.TrailingEmptyItem = false;
                this.Current = default;
                return true;
            }

            if (this.Span.IsEmpty)
            {
                this.Span = this.Current = default;
                return false;
            }

            int idx = this.Span.IndexOf(this.SeparatorSequence);
            if (idx < 0)
            {
                this.Current = this.Span;
                this.Span = default;
            }
            else
            {
                this.Current = this.Span.Slice(0, idx);
                this.Span = this.Span.Slice(idx + this.SeparatorSequence.Length);
                if (this.Span.IsEmpty)
                    this.TrailingEmptyItem = true;
            }

            return true;
        }

        public ReadOnlySpan<T> Current { get; private set; }
    }

    [Pure]
    public static Enumerable1<T> Split<T>(this ReadOnlySpan<T> span, T separator)
        where T : IEquatable<T> => new(span, separator);

    [Pure]
    public static Enumerable2<T> SplitMultiple<T>(this ReadOnlySpan<T> span, T separator1, T separator2)
        where T : IEquatable<T> => new(span, separator1, separator2);

    [Pure]
    public static Enumerable3<T> SplitMultiple<T>(this ReadOnlySpan<T> span, T separator1, T separator2, T separator3)
        where T : IEquatable<T> => new(span, separator1, separator2, separator3);

    [Pure]
    public static EnumerableN<T> SplitMultiple<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values)
        where T : IEquatable<T> => new(span, values);

    [Pure]
    public static EnumerableSequence<T> Split<T>(this ReadOnlySpan<T> span, ReadOnlySpan<T> values)
        where T : IEquatable<T> => new(span, values);
}
