using BenchmarkDotNet.Attributes;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;

namespace LtQueryBenchmarks.Benchmarks;

[Config(typeof(BenchmarkConfig))]
public class JoinBenchmark : AbstractBenchmark
{
    SqlConnection _connection;

    [GlobalSetup]
    public void Setup()
    {
        _connection = new SqlConnection(Constants.ConnectionString);
        _connection.Open();
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _connection.Dispose();
    }


    const string _normalSql = "SELECT t1.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content] FROM [Blog] AS t1 INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId]";

    [Benchmark]
    public int NormalJoin()
    {
        var accum = 0;

        var entities = new List<Post>();
        {
            var command = new SqlCommand(_normalSql, _connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var post = new Post(reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                    entities.Add(post);
                }
            }
        }

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }


    const string _subQuerySql = "SELECT _.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content] FROM (SELECT t1.[Id] FROM [Blog] AS t1) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId]";

    [Benchmark]
    public int SubQuery()
    {
        var accum = 0;

        var entities = new List<Post>();
        {
            var command = new SqlCommand(_subQuerySql, _connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var post = new Post(reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                    entities.Add(post);
                }
            }
        }

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }
}
