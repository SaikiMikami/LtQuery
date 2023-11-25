using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational.Tests;

public class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryRelational();

        collection.AddSingleton<ISqlBuilder, TestSqlBuilder>();
        collection.AddTestBySqlServer();

        return collection.BuildServiceProvider();
    }
}
