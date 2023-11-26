using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace LtQuery.MySql.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryMySql(new ModelConfiguration(), _ => new MySqlConnection(Constants.MySqlConnectionString));

        return collection.BuildServiceProvider();
    }
}
