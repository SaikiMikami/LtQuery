using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational.Tests;

public class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryRelational(new ModelConfiguration(), _ => new SqlConnection(Constants.SqlServerConnectionString));
        collection.AddSingleton<ISqlBuilder, TestSqlBuilder>();

        return collection.BuildServiceProvider();
    }
}
