namespace LtQuery.Relational.Nodes.Values;

public interface IBinaryOperatorData : IBoolValueData
{
    IValueData Lhs { get; }
    IValueData Rhs { get; }
}
