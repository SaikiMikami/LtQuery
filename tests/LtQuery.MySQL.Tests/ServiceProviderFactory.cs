using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.MySql.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryMyServer();

        collection.AddTestByMySql();

        return collection.BuildServiceProvider();
    }
}
