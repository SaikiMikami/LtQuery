// See https://aka.ms/new-console-template for more information
using LtQuery.SqlServer;
using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Sql.Utils;


public class Arg
{
    public int Id { get; init; }
}
internal class Program
{
    static void Main(string[] args)
    {
        switch (Console.ReadLine())
        {
            case "1":
                createReader();
                break;
        }
    }

    static Query<Blog> _includeChilrenQuery = Lt.Query<Blog>().Include(_ => _.Posts).Where(_ => _.Id < Lt.Arg<int>("Id")).ToImmutable();
    static void createReader()
    {
        var provider = create();
        using (var scope = provider.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<ILtConnection>();

            var entities = connection.Select(_includeChilrenQuery, new Arg { Id = 20 });
        }
    }
    static IServiceProvider create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlServer();

        collection.AddTest();

        return collection.BuildServiceProvider();
    }
}

