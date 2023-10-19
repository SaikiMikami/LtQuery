using LtQuery.Metadata;
using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.Sql.Tests;

public class RepositoryTests
{
    readonly IServiceProvider _provider;
    readonly EntityMetaService _metaService;
    public RepositoryTests()
    {
        _provider = new ServiceProviderFactory().Create();
        var modelConfiguration = _provider.GetRequiredService<IModelConfiguration>();

        _metaService = new EntityMetaService(modelConfiguration);
    }

    [Fact]
    public void Select()
    {
        var query = new Query<Blog>(null, null, null);

        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;

            var sqlBuilder = _provider.GetRequiredService<ISqlBuilder>();
            var repository = new Repository<Blog>(_metaService, sqlBuilder);

            var connection = _provider.GetRequiredService<DbConnection>();
            connection.Open();
            var array = repository.Select(connection, query);
        }
    }
}
