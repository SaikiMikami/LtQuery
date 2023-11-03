using LtQuery.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational;

public static class ServiceCollectionExtensions
{
    public static void AddLtQuerySql(this IServiceCollection _this)
    {
        _this.AddSingleton<EntityMetaService>();
        _this.AddScoped<ILtConnection, LtConnection>();
    }
}