namespace LtQuery.Elements.Values.Operators;

public sealed class LessThanOrEqualOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<LessThanOrEqualOperator>
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public LessThanOrEqualOperator(IValue lhs, IValue rhs)
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

    public override bool Equals(object? obj) => Equals(obj as LessThanOrEqualOperator);
    public bool Equals(LessThanOrEqualOperator? other)
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