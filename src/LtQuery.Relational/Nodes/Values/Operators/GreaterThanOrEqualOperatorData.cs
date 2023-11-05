namespace LtQuery.Relational.Nodes.Values.Operators;

public class GreaterThanOrEqualOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public GreaterThanOrEqualOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }
}