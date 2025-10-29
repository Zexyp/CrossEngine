using System;
using System.Collections.Generic;

namespace CrossEngine.Utils;

// pot of boiling shit
class InterfaceTypeComparer<T> : IEqualityComparer<Type>
{
    public bool Equals(Type x, Type y)
    {
        if (x == y) return true;
        if (x.IsAssignableFrom(y)) return true;
        return false;
    }

    public int GetHashCode(Type obj)
    {
        if (obj.IsInterface)
            return obj.GetHashCode();

        Type baseInterface = null;
        var ints = obj.GetInterfaces();
        for (int i = ints.Length - 1; i >= 0; i--)
        {
            if (!typeof(T).IsAssignableFrom(ints[i]))
                continue;

            baseInterface = ints[i];
            break;
        }
        return baseInterface?.GetHashCode() ?? obj.GetHashCode();
    }
}