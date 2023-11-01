namespace LtQuery.Elements;

public sealed class ConstantValue : AbstractImmutable, IValue
{
    public string? Value { get; }
    public ConstantValue(string? value)
    {
        Value = value;
    }

    protected override int CreateHashCode() => Value?.GetHashCode() ?? 0;
}
