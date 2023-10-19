namespace LtQuery.Elements.Values.Operators;

public sealed class GreaterThanOperator : AbstractImmutable, IBinaryOperator, IBoolValue
{
    public IValue Lhs { get; }
    public IValue Rhs { get; }
    public GreaterThanOperator(IValue lhs, IValue rhs)
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
