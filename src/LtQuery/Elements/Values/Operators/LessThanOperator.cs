namespace LtQuery.Elements.Values.Operators;

public sealed class LessThanOperator : AbstractImmutable, IBinaryOperator, IBoolValue
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
}
