using System.Collections;

namespace LtQuery;

#pragma warning disable CS0659
public class ImmutableArray<T> : AbstractImmutable, IReadOnlyList<T>, IEquatable<ImmutableArray<T>> where T : IImmutable
#pragma warning restore CS0659
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
        AddHashCode(ref code, length);
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

    public override bool Equals(object? obj) => Equals(obj as Query<ImmutableArray<T>>);
    public bool Equals(ImmutableArray<T>? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;

        if (Count != other.Count)
            return false;
        for (var i = 0; i < Count; i++)
        {
            if (!Equals(this[i], other[i]))
                return false;
        }
        return true;
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
