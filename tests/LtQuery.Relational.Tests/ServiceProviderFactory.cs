using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.Relational.Tests;

public class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQueryRelational();

        collection.AddSingleton<ISqlBuilder, TestSqlBuilder>();
        collection.AddTest();
        collection.AddScoped<DbConnection>(_ => new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=LtQueryTest"));

        return collection.BuildServiceProvider();
    }
}
