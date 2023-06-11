using System;
using System.Collections.Concurrent;
using System.Text;

namespace LR.Standard;

public class ObjectPool<T>
{
    private ConcurrentBag<T> _objects;
    private Func<T> _factory;

    public ObjectPool(Func<T> factory)
    {
        _objects = new ConcurrentBag<T>();
        _factory = factory ?? throw new ArgumentNullException("objectGenerator");
    }

    public T GetObject()
    {
        if (_objects.TryTake(out T item)) return item;
        return _factory();
    }

    public void PutObject(T item)
    {
        _objects.Add(item);
    }
}
