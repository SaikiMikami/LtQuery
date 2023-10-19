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

        collection.AddTest();

        return collection.BuildServiceProvider();
    }

    IServiceScope _scope;
    ILtConnection _connection;
    static Query<Blog> _singleQuery = Lt.Query<Blog>().Where(_ => _.Id == 1).ToImmutable();
    static Query<Blog> _selectSimpleQuery = Lt.Query<Blog>().OrderBy(_ => _.Id).Take(20).ToImmutable();
    static Query<Blog> _includeUniqueManyQuery = Lt.Query<Blog>().Include(_ => _.Posts).ToImmutable();
    static Query<Blog> _includeChilrenQuery = Lt.Query<Blog>().Where(_ => _.Id < Lt.Arg<int>("Id")).Include(_ => _.Posts).ToImmutable();
    public void Setup()
    {
        var provider = Create();
        _scope = provider.CreateScope();
        provider = _scope.ServiceProvider;

        _connection = provider.GetRequiredService<ILtConnection>();
        _connection.Select(_singleQuery);
        _connection.Select(_selectSimpleQuery);
        _connection.Select(_includeUniqueManyQuery);
        _connection.Select(_includeChilrenQuery, new { Id = 20 });
    }
    public void Cleanup()
    {
        _scope.Dispose();
    }

    public int SelectSingle()
    {
        var entity = _connection.Single(_singleQuery);

        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }

    public int SelectSimple()
    {
        var accum = 0;

        var entities = _connection.Select(_selectSimpleQuery);

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    public int SelectAllIncludeUniqueMany()
    {
        var accum = 0;

        var entities = _connection.Select(_includeUniqueManyQuery);

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    public int SelectIncludeChilren()
    {
        var accum = 0;

        var entities = _connection.Select(_includeChilrenQuery, new { Id = 20 });

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }
}
