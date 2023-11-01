using System.Text;

namespace LtQuery.SqlServer.Values.Operators;

class LessThanOrEqualOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public LessThanOrEqualOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    public StringBuilder Append(StringBuilder strb)
    {
        Lhs.Append(strb).Append(" <= ");
        Rhs.Append(strb);
        return strb;
    }
}