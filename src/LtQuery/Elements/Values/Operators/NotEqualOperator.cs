namespace LtQuery.Elements.Values.Operators;

public sealed class NotEqualOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<NotEqualOperator>
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public NotEqualOperator(IValue lhs, IValue rhs)
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

    public override bool Equals(object? obj) => Equals(obj as NotEqualOperator);
    public bool Equals(NotEqualOperator? other)
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