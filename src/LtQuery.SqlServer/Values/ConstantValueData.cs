using System.Text;

namespace LtQuery.SqlServer.Values;

class ConstantValueData : IValueData
{
    public string? Value { get; }
    public ConstantValueData(string? value)
    {
        Value = value;
    }

    public StringBuilder Append(StringBuilder strb) => strb.Append(Value);
}
