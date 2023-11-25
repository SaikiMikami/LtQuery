using LtQuery;
using LtQuery.SqlServer;
using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQueryBenchmarks.LtQuery;

class LtQueryBenchmark : AbstractBenchmark
{
    public IServiceProvider Create()
    {
        var collection = new ServiceCollection();
        collection.AddLtQuerySqlServer();

        collection.AddTestBySqlServer();

        return collection.BuildServiceProvider();
    }

    IServiceScope _scope = default!;
    ILtConnection _connection = default!;
    public void Setup()
    {
        var provider = Create();
        _scope = provider.CreateScope();
        provider = _scope.ServiceProvider;

        _connection = provider.GetRequiredService<ILtConnection>();
        _connection.Select(_singleQuery);
        _connection.Select(_selectSimpleQuery);
        _connection.Select(_includeChilrenQuery, new { Id = 20 });
    }
    public void Cleanup()
    {
        _scope.Dispose();
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
}
