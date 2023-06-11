using System;
using System.Collections.Generic;
using System.Collections;

namespace LR.Standard;

public sealed class SingleCollectionAndEnumerator<T> : IEnumerable<T>, IEnumerator<T>
{
    public SingleCollectionAndEnumerator(T value) => this.Value = value;
    public readonly T Value;
    private byte _state;
    public T Current => this.Value;
    object? IEnumerator.Current => this.Current;
    public bool MoveNext()
    {
        if (this._state == 0)
        {
            this._state = 1;
            return true;
        }
        else if (this._state == 1)
        {
            this._state = byte.MaxValue;
            return false;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
    public IEnumerator<T> GetEnumerator() => this;
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    public void Reset() => this._state = 0;
    public void Dispose() { }
}
