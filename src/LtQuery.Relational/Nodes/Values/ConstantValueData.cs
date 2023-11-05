namespace LtQuery.Relational.Nodes.Values;

public class ConstantValueData : IValueData
{
    public string? Value { get; }
    public ConstantValueData(string? value)
    {
        Value = value;
    }
}
