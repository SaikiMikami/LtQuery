namespace LtQuery.Relational.Nodes.Values.Operators;

public class AndAlsoOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public AndAlsoOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }
}
