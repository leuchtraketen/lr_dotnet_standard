using System.Collections.Generic;

namespace LR.Standard;

public class HiddenReference<T>
{
    // Analysis disable once StaticFieldInGenericType
    private static readonly Dictionary<int, T> table = new();

    // Analysis disable once StaticFieldInGenericType
    private static int idgen = 0;

    private int id;

    public HiddenReference()
    {
        lock (table)
        {
            this.id = idgen++;
        }
    }

    ~HiddenReference()
    {
        lock (table)
        {
            table.Remove(this.id);
        }
    }

    public T Value
    {
        get
        {
            lock (table)
            {
                return table.TryGetValue(this.id, out T res) ? res : default!;
            }
        }
        set
        {
            lock (table)
            {
                table[this.id] = value;
            }
        }
    }
}
