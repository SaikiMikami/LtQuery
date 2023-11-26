using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.SqlServer.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlServer(new ModelConfiguration(), _ => new SqlConnection(Constants.SqlServerConnectionString));

        return collection.BuildServiceProvider();
    }
}
