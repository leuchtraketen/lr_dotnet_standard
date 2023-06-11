using System;
using System.Collections.Immutable;

namespace LR.Standard;

public static class InterlockedExtensions
{
    public static void InterlockedUpdate<T, TArg>(this ref ImmutableArray<T> array, Func<ImmutableArray<T>, TArg, ImmutableArray<T>> update, TArg argument)
    {
        while (true)
        {
            ImmutableArray<T> original = array;
            ImmutableArray<T> updated = update(original, argument);
            if (original == updated || ImmutableInterlocked.InterlockedCompareExchange(ref array, updated, original) == original)
            {
                return;
            }
        }
    }

    public static void InterlockedUpdate<T, TArg>(this ref ImmutableArray<T> array, Func<ImmutableArray<T>, TArg, ImmutableArray<T>> update, TArg argument,
        out ImmutableArray<T> original)
    {
        while (true)
        {
            original = array;
            ImmutableArray<T> updated = update(original, argument);
            if (original == updated || ImmutableInterlocked.InterlockedCompareExchange(ref array, updated, original) == original)
            {
                return;
            }
        }
    }
}
