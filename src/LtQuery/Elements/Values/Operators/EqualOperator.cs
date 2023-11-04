namespace LtQuery.Elements.Values.Operators;

#pragma warning disable CS0659
public sealed class EqualOperator : AbstractImmutable, IBinaryOperator, IBoolValue, IEquatable<EqualOperator>
#pragma warning restore CS0659
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public EqualOperator(IValue lhs, IValue rhs)
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

    public override bool Equals(object? obj) => Equals(obj as EqualOperator);
    public bool Equals(EqualOperator? other)
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
