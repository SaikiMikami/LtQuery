namespace LtQuery;

public abstract class AbstractImmutable : IImmutable
{
    int? _hashCode;
    public override int GetHashCode()
    {
        _hashCode ??= CreateHashCode();
        return _hashCode.Value;
    }
    protected abstract int CreateHashCode();

    protected static int AddHashCode(ref int code, object? value) => code = unchecked(code * 5 ^ (value?.GetHashCode() ?? 0));
}
