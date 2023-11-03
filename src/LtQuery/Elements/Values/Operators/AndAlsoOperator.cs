namespace LtQuery.Elements.Values.Operators;

public sealed class AndAlsoOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<AndAlsoOperator>
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public AndAlsoOperator(IValue lhs, IValue rhs)
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

    public override bool Equals(object? obj) => Equals(obj as AndAlsoOperator);
    public bool Equals(AndAlsoOperator? other)
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
