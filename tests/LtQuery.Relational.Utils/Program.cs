// See https://aka.ms/new-console-template for more information
using LtQuery.SqlServer;
using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Sql.Utils;


public class Arg
{
    public string UserName { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
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

    static Query<Blog> _complexQuery = Lt.Query<Blog>().Include(_ => _.User).Include(new[] { "Posts", "User" }).Where(_ => _.Posts.Any(_ => _.User.Name == Lt.Arg<string>("UserName"))).OrderBy(_ => _.Id).Skip("Skip").Take("Take").ToImmutable();
    static void createReader()
    {
        var provider = create();
        using (var scope = provider.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<ILtConnection>();

            var entities = connection.Select(_complexQuery, new Arg { UserName = "PLCJKJKRUK", Skip = 20, Take = 20 });
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

