namespace LtQuery.Elements.Values;

public interface IBinaryOperator : IValue
{
    IValue Lhs { get; }
    IValue Rhs { get; }
}
