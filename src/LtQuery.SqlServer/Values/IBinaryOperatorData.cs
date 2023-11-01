namespace LtQuery.SqlServer.Values;

interface IBinaryOperatorData : IBoolValueData
{
    IValueData Lhs { get; }
    IValueData Rhs { get; }
}
