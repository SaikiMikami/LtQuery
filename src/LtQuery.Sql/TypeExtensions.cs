namespace LtQuery.Sql;

static class TypeExtensions
{
    public static bool IsNullable(this Type _this)
    {
        if (!_this.IsGenericType)
            return false;
        return _this.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
