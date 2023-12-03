using LtQuery.SqlServer;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Relational.Utils;

public class Arg
{
    public string UserName { get; init; } = default!;
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
                select();
                break;
            case "2":
                var task = selectAsync();
                task.AsTask().Wait();
                break;
        }
    }

    //static Query<Blog> _simpleQuery = Lt.Query<Blog>().ToImmutable();
    //static Query<Blog> _simpleQuery2 = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();
    static Query<Blog> _complexQuery = Lt.Query<Blog>().Include(_ => _.User).Include<User>(new[] { "Posts", "User" }).Where(_ => _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName"))).OrderBy(_ => _.Id).Skip("Skip").Take("Take").ToImmutable();
    static void select()
    {
        var provider = create();
        using (var scope = provider.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<ILtConnection>();

            //var entities = await connection.SelectAsync(_simpleQuery);
            //var entities = await connection.SelectAsync(_simpleQuery2, new Arg2 { Id = 1 });
            //var entities = connection.Select(_complexQuery, new Arg { UserName = "PLCJKJKRUK", Skip = 20, Take = 20 });
        }
    }
    static async ValueTask selectAsync()
    {
        var provider = create();
        using (var scope = provider.CreateScope())
        {
            var connection = scope.ServiceProvider.GetRequiredService<ILtConnection>();

            //var entities = await connection.SelectAsync(_simpleQuery);
            //var entities0 = await connection.SelectAsync(_simpleQuery2, new Arg2 { Id = 1 });
            var entities = await connection.SelectAsync(_complexQuery, new { UserName = "PLCJKJKRUK", Skip = 20, Take = 20 });
        }
    }

    static IServiceProvider create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlServer(new ModelConfiguration(), _ => new SqlConnection(Constants.SqlServerConnectionString));

        return collection.BuildServiceProvider();
    }
}

