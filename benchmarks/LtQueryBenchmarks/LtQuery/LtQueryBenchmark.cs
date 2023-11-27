using LtQuery;
using LtQuery.SqlServer;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace LtQueryBenchmarks.LtQuery;

class LtQueryBenchmark : AbstractBenchmark
{
    RandomEx _random = default!;
    IServiceScope _scope = default!;
    ILtConnection _connection = default!;
    public void Setup()
    {
        _random = new(0);
        var provider = CreateProvider();
        _scope = provider.CreateScope();
        provider = _scope.ServiceProvider;

        _connection = provider.GetRequiredService<ILtConnection>();
        _connection.Select(_singleQuery);
        _connection.Select(_selectSimpleQuery);
        _connection.Select(_includeChilrenQuery, new { Id = 20 });
        _connection.Add(new Tag("a"));
    }
    public void Cleanup()
    {
        _scope.Dispose();
    }

    public IServiceProvider CreateProvider()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlServer(new ModelConfiguration(), _ => new SqlConnection(@"Server=(localdb)\MSSQLLocalDB;Database=LtQueryTest"));

        return collection.BuildServiceProvider();
    }


    static Query<Blog> _singleQuery = Lt.Query<Blog>().Where(_ => _.Id == 1).ToImmutable();

    public int SelectSingle()
    {
        var entity = _connection.Single(_singleQuery);

        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }


    static Query<Blog> _selectSimpleQuery = Lt.Query<Blog>().OrderBy(_ => _.Id).Take(20).ToImmutable();

    public int SelectSimple()
    {
        var entities = _connection.Select(_selectSimpleQuery);

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }


    static Query<Blog> _includeChilrenQuery = Lt.Query<Blog>().Include(_ => _.Posts).Where(_ => _.Id < Lt.Arg<int>("Id")).ToImmutable();

    public int SelectIncludeChilren()
    {
        var entities = _connection.Select(_includeChilrenQuery, new { Id = 20 });

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }

    static Query<Blog> _complexQuery = Lt.Query<Blog>().Include(_ => _.User)
        .Include(_ => _.Posts).ThenInclude(_ => _.User)
        .Where(_ => _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName"))).OrderBy(_ => _.Id).Skip("Skip").Take("Take").ToImmutable();

    public int SelectComplex()
    {
        var entities = _connection.Select(_complexQuery, new { UserName = "PLCJKJKRUK", Skip = 20, Take = 20 });

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }

    public int AddRange()
    {
        using (var unitOfWork = _connection.CreateUnitOfWork())
        {
            var tags = new List<Tag>();
            for (var i = 0; i < 10; i++)
            {
                tags.Add(new(_random.NextString()));
            }
            unitOfWork.AddRange(tags);
            unitOfWork.Commit();
        }
        return 0;
    }
}
