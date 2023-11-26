using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SQLite;

namespace LtQuery.Sqlite.Tests;

class ServiceProviderFactory
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlite(new ModelConfiguration(), _ => new SQLiteConnection(Constants.SqliteConnectionString));

        return collection.BuildServiceProvider();
    }
}
