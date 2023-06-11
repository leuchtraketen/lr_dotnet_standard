using System;

namespace LR.Standard;

public interface IDisposableWrapper<T> : IDisposable
{
    T Value { get; set; }
}

public sealed class DisposableWrapper<T> : IDisposableWrapper<T> where T : class, IDisposable
{
    public T Value { get; set; }

    public void Dispose()
    {
        if (this.Value != null)
        {
            this.Value.Dispose();
            this.Value = null;
        }
    }
}

public sealed class UndisposableWrapper<T> : IDisposableWrapper<T> where T : class, IDisposable
{
    public T Value { get; set; }

    public void Dispose()
    {
        // don't dispose anything!
    }
}
