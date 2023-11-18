using LtQuery.Metadata;

namespace LtQuery.Tests.Metadata;

public class TypeExtensionsTests
{
    [Fact]
    public void IsNullable()
    {
        Assert.False(typeof(int).IsNullable());
        Assert.True(typeof(int?).IsNullable());
    }

    class ClassA
    {
        public string NotNullable { get; } = default!;
        public string? Nullable { get; }
    }

    [Fact]
    public void IsNullableReference()
    {
        var property = typeof(ClassA).GetProperty(nameof(ClassA.NotNullable))!;
        Assert.False(property.IsNullableReference());

        property = typeof(ClassA).GetProperty(nameof(ClassA.Nullable))!;
        Assert.True(property.IsNullableReference());
    }
}
