using System.Collections;

namespace LtQuery;

public class ImmutableArray<T> : AbstractImmutable, IReadOnlyList<T> where T : IImmutable
{
    readonly T[] _values;
    public ImmutableArray(T[] values)
    {
        _values = values;
    }

    public T this[int index] => _values[index];

    public int Count => _values.Length;

    public IEnumerator<T> GetEnumerator() => new ArrayEnumerator<T>(_values);

    protected override int CreateHashCode()
    {
        var code = 0;
        var length = _values.Length;
        if (length >= 1)
        {
            AddHashCode(ref code, _values[0]);

            if (length >= 2)
            {
                AddHashCode(ref code, _values[length - 1]);
                if (length >= 3)
                {
                    AddHashCode(ref code, _values[(length - 1) / 2]);
                }
            }
        }
        return code;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


}
public class ArrayEnumerator<T> : IEnumerator<T>
{
    T[] _parent;
    public ArrayEnumerator(T[] parent)
    {
        _parent = parent;
    }

    public void Dispose() { }

    int _index = -1;

    public T Current => _parent[_index];

    object IEnumerator.Current => Current!;

    public bool MoveNext()
    {
        _index++;
        return _index < _parent.Length;
    }

    public void Reset()
    {
        _index = -1;
    }
}
