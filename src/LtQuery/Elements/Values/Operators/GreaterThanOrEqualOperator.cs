namespace LtQuery.Elements.Values.Operators;

public sealed class GreaterThanOrEqualOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<GreaterThanOrEqualOperator>
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public GreaterThanOrEqualOperator(IValue lhs, IValue rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    protected override int CreateHashCode()
    {
        var code = 0;
        AddHashCode(ref code, Lhs);
        AddHashCode(ref code, Rhs);
        return code;
    }

    public override bool Equals(object? obj) => Equals(obj as GreaterThanOrEqualOperator);
    public bool Equals(GreaterThanOrEqualOperator? other)
    {
        if (other == null)
            return false;

        if (!Lhs.Equals(other.Lhs))
            return false;
        if (!Rhs.Equals(other.Rhs))
            return false;
        return true;
    }
}