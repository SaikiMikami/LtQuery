namespace LtQuery.Elements;

public sealed class ConstantValue : AbstractImmutable, IValue, IEquatable<ConstantValue>
{
    public string? Value { get; }
    public ConstantValue(string? value)
    {
        Value = value;
    }

    protected override int CreateHashCode() => Value?.GetHashCode() ?? 0;

    public override bool Equals(object? obj) => Equals(obj as ConstantValue);
    public bool Equals(ConstantValue? other)
    {
        if (other == null)
            return false;

        if (!Equals(Value, other.Value))
            return false;
        return true;
    }
}
