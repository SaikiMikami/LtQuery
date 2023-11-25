using LtQuery.Relational;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.MySql;

public static class ServiceCollectionExtensions
{
    public static void AddLtQueryMyServer(this IServiceCollection _this)
    {
        _this.AddLtQueryRelational();
        _this.AddSingleton<ISqlBuilder, SqlBuilder>();
    }
}
