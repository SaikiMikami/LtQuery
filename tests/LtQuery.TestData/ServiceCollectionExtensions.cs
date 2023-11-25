using LtQuery.Metadata;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Data.Common;

namespace LtQuery.TestData;

public static class ServiceCollectionExtensions
{
    public static void AddTestBySqlServer(this IServiceCollection _this)
    {
        _this.AddSingleton<IModelConfiguration, ModelConfiguration>();
        _this.AddScoped<DbConnection>(_ => new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=LtQueryTest"));
    }
    public static void AddTestByMySql(this IServiceCollection _this)
    {
        _this.AddSingleton<IModelConfiguration, ModelConfiguration>();
        _this.AddScoped<DbConnection>(_ => new MySqlConnection(@"server=localhost;user=ltquerytest;database=ltquerytest"));
    }
}