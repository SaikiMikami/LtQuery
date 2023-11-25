using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Data.SQLite;

namespace LtQuery.Sqlite.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlite();
        collection.AddTest();
        collection.AddScoped<DbConnection>(_ => new SQLiteConnection(@"Data Source=LtQueryTest.db"));

        return collection.BuildServiceProvider();
    }
}
