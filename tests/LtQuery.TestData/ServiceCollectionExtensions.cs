using LtQuery.Metadata;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;

namespace LtQuery.TestData;

public static class ServiceCollectionExtensions
{
    public static void AddTest(this IServiceCollection _this)
    {
        _this.AddSingleton<IModelConfiguration, ModelConfiguration>();
        _this.AddScoped<DbConnection>(_ => new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=LtQueryTest"));
    }
}