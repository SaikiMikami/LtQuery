using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Sql.Tests;

public class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySql();

        collection.AddSingleton<ISqlBuilder, TestSqlBuilder>();
        collection.AddTest();

        return collection.BuildServiceProvider();
    }
}
