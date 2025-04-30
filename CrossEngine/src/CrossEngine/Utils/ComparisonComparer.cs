using System;
using System.Collections;

namespace CrossEngine.Utils;

public class ComparisonComparer<T> : IComparer
{
    private readonly Comparison<T> _comparison;

    public ComparisonComparer(Comparison<T> comparison)
    {
        _comparison = comparison;
    }

    public int Compare(T x, T y)
    {
        return _comparison(x, y);
    }

    public int Compare(object o1, object o2)
    {
        return _comparison((T)o1, (T)o2);
    }
}