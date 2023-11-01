using LtQuery.Sql;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.SqlServer;

public static class ServiceCollectionExtensions
{
    public static void AddLtQuerySqlServer(this IServiceCollection _this)
    {
        _this.AddLtQuerySql();
        _this.AddSingleton<ISqlBuilder, SqlBuilder>();
    }
}
