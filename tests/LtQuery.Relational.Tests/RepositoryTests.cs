using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational.Tests;

public class RepositoryTests
{
    readonly IServiceProvider _provider;
    public RepositoryTests()
    {
        _provider = new ServiceProviderFactory().Create();
    }

    [Fact]
    public void Select()
    {
        var query = new Query<Blog>(null, null, null);

        using (var scope = _provider.CreateScope())
        {
            var provider = scope.ServiceProvider;

            var repository = _provider.GetRequiredService<IRepository<Blog>>();

            var connection = _provider.GetRequiredService<ILtConnection>();
            var array = repository.Select((LtConnection)connection, query);
        }
    }
}
