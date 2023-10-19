using System.Text;

namespace LtQuery.SqlServer.Values.Operators;

class NotEqualOperatorData : IBinaryOperatorData
{
    public IValueData Lhs { get; }
    public IValueData Rhs { get; }
    public NotEqualOperatorData(IValueData lhs, IValueData rhs)
    {
        Lhs = lhs;
        Rhs = rhs;
    }

    public StringBuilder Append(StringBuilder strb)
    {
        Lhs.Append(strb).Append(" != ");
        Rhs.Append(strb);
        return strb;
    }
}
