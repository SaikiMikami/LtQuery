using LtQuery.Metadata;

namespace LtQuery.Tests.Metadata;

public class TypeExtensionsTests
{
    public void IsNullable()
    {
        Assert.False(typeof(int).IsNullable());
        Assert.True(typeof(int?).IsNullable());
    }
}
