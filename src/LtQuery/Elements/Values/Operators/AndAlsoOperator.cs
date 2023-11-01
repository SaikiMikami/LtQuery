namespace LtQuery.Elements.Values.Operators;

public sealed class AndAlsoOperator : AbstractImmutable, IBinaryOperator, IBoolValue
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
}
