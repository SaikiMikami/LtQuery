using System.Reflection;

namespace LtQuery.Relational;

static class TypeExtensions
{
    public static bool IsNullable(this Type _this)
    {
        if (!_this.IsGenericType)
            return false;
        return _this.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    public static bool IsNullableReference(this PropertyInfo _this)
    {
        var nullabilityInfoContext = new NullabilityInfoContext();
        switch (nullabilityInfoContext.Create(_this).ReadState)
        {
            case NullabilityState.Nullable:
                return true;
            case NullabilityState.NotNull:
                return false;
            default:
                throw new InvalidOperationException("Nullable reference must be enabled");
        }
    }
}
