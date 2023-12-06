using LtQuery.Metadata;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.Relational;

public static class ServiceCollectionExtensions
{
    public static void AddLtQueryRelational(this IServiceCollection _this, IModelConfiguration modelConfiguration, Func<IServiceProvider, DbConnection> createDbConnectionFunc, LtSettings? settings = default)
    {
        _this.AddSingleton(typeof(IRepository<>), typeof(Repository<>));
        _this.AddSingleton<EntityMetaService>();
        _this.AddSingleton<DbConnectionPool>();
        _this.AddSingleton(modelConfiguration);
        _this.AddTransient(createDbConnectionFunc);

        _this.AddSingleton(settings ?? new());

        _this.AddScoped<ILtConnection, LtConnection>();
        _this.AddScoped(_ => _.GetRequiredService<ILtConnection>().CreateUnitOfWork());
    }
}