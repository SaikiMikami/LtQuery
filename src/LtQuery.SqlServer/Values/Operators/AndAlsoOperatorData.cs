using System.Text;

namespace LtQuery.SqlServer.Values.Operators;

class AndAlsoOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public AndAlsoOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    public StringBuilder Append(StringBuilder strb)
    {
        Lhs.Append(strb).Append(" AND ");
        Rhs.Append(strb);
        return strb;
    }
}
