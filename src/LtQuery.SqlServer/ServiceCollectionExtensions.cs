using LtQuery.Metadata;
using LtQuery.Relational;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.SqlServer;

public static class ServiceCollectionExtensions
{
    public static void AddLtQuerySqlServer(this IServiceCollection _this, IModelConfiguration modelConfiguration, Func<IServiceProvider, DbConnection> createDbConnectionFunc, LtSettings? settings = default)
    {
        _this.AddLtQueryRelational(modelConfiguration, createDbConnectionFunc, settings);
        _this.AddSingleton<ISqlBuilder, SqlBuilder>();
    }
}
