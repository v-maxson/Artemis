using System;

namespace NWDB;

public class ConcurrentBool
{
    private int _backValue;

    public bool Value {
        get {
            return Interlocked.CompareExchange(ref _backValue, 1, 1) == 1;
        }
        set {
            if (value) Interlocked.CompareExchange(ref _backValue, 1, 0);
            else Interlocked.CompareExchange(ref _backValue, 0, 1);
        }
    }


}
