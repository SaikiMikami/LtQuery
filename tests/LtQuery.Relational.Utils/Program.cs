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

    static Query<Blog> _includeChilrenQuery = Lt.Query<Blog>().Where(_ => _.Id < Lt.Arg<int>("Id")).Include(_ => _.Posts).ToImmutable();
    static void createReader()
    {
        var provider = create();
        using (var scope = provider.CreateScope())
        {
            provider = scope.ServiceProvider;

            var connection = provider.GetRequiredService<ILtConnection>();

            var entities = connection.Select(_includeChilrenQuery, new Arg { Id = 20 });
            if (entities.Count != 1)
                throw new Exception();
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

