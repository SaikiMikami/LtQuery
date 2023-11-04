namespace LtQuery.Elements.Values.Operators;

#pragma warning disable CS0659
public sealed class LessThanOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<LessThanOperator>
#pragma warning restore CS0659
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public LessThanOperator(IValue lhs, IValue rhs)
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

    public override bool Equals(object? obj) => Equals(obj as LessThanOperator);
    public bool Equals(LessThanOperator? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other == null)
            return false;

        if (!Lhs.Equals(other.Lhs))
            return false;
        if (!Rhs.Equals(other.Rhs))
            return false;
        return true;
    }
}
