using System;

namespace LR.Standard;

public class Factory<T>
{
    readonly Func<T> construct;

    public Factory(Func<T> construct)
    {
        this.construct = construct;
    }

    public virtual T Instance => this.construct();
}

public class SingletonFactory<T> : Factory<T> where T : class
{
    T singleton;

    public SingletonFactory(Func<T> construct) : base(construct: construct)
    {
    }

    public override T Instance => ( this.singleton == null ) ? ( this.singleton = base.Instance ) : ( this.singleton );
}
