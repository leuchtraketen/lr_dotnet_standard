using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LR.Standard;

public abstract class ProducerConsumerQueue
{
    private static long _id_max;

    protected readonly long _id;

    protected ProducerConsumerQueue()
    {
        _id = Interlocked.Increment(ref _id_max);
    }

    public abstract void Produce(Action value);
    public abstract void Shutdown();
}
