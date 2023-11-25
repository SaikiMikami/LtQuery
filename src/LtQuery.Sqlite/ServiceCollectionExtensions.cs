using LtQuery.Relational;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Sqlite;

public static class ServiceCollectionExtensions
{
    public static void AddLtQuerySqlite(this IServiceCollection _this)
    {
        _this.AddLtQueryRelational();
        _this.AddSingleton<ISqlBuilder, SqlBuilder>();
    }
}
