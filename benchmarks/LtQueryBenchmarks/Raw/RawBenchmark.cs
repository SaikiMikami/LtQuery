using LtQuery.TestData;
using Microsoft.Data.SqlClient;

namespace LtQueryBenchmarks.Raw;

class RawBenchmark : AbstractBenchmark
{
    SqlConnection _connection;
    public void Setup()
    {
        _connection = new SqlConnection(Constants.ConnectionString);
        _connection.Open();
    }

    public void Cleanup()
    {
        _connection.Dispose();
    }

    const string _singleSql = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] = 1";
    public int SelectSingle()
    {
        Blog entity;
        using (var command = new SqlCommand(_singleSql, _connection))
        {
            using (var reader = command.ExecuteReader())
            {
                if (!reader.Read())
                    throw new InvalidOperationException("not data");

                entity = new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));

                if (reader.Read())
                    throw new InvalidOperationException("multi");
            }
        }
        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }

    const string _selectSimpleSql = "SELECT TOP(20) [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] ORDER BY [Id]";
    public int SelectSimple()
    {
        var entities = new List<Blog>();
        using (var command = new SqlCommand(_selectSimpleSql, _connection))
        {

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    entities.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5)));
                }
            }
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    const string _allIncludeUniqueManySql1 = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog]";
    const string _allIncludeUniqueManySql12 = "SELECT t1.[Id], t2.[Id], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId]";
    public int SelectAllIncludeUniqueMany()
    {
        var entities = new List<Blog>();
        var blogDic = new Dictionary<int, Blog>();

        using (var command = new SqlCommand(_allIncludeUniqueManySql1, _connection))
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                var blog = new Blog(id, reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                entities.Add(blog);
                blogDic.Add(id, blog);
            }
        }

        using (var command = new SqlCommand(_allIncludeUniqueManySql12, _connection))
        using (var reader = command.ExecuteReader())
        {
            Blog blog = default!;
            var preId = 0;
            while (reader.Read())
            {
                var arg0 = reader.GetInt32(0);
                if (preId != arg0)
                {
                    blog = blogDic[arg0];
                    preId = arg0;
                }

                int? arg2;
                if (reader.IsDBNull(2))
                    arg2 = null;
                else
                    arg2 = reader.GetInt32(2);

                var post = new Post(reader.GetInt32(1), arg0, arg2, reader.GetDateTime(3), reader.GetString(4));
                post.Blog = blog;
                blog.Posts.Add(post);
            }
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    const string _selectIncludeChilrenSql1 = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] < @Id";
    const string _selectIncludeChilrenSql2 = "SELECT _.[Id], t2.[Id], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1 WHERE [Id] < @Id) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId]";
    public int SelectIncludeChilren()
    {
        var accum = 0;

        var entities = new List<Blog>();
        var blogDic = new Dictionary<int, Blog>();
        using (var command = new SqlCommand(_selectIncludeChilrenSql1, _connection))
        {

            var p1 = command.CreateParameter();
            p1.ParameterName = "@Id";
            p1.DbType = System.Data.DbType.Int32;
            p1.Value = 20;
            command.Parameters.Add(p1);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var blog = new Blog(id, reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                    entities.Add(blog);
                    blogDic.Add(id, blog);
                }
            }
        }

        using (var command = new SqlCommand(_selectIncludeChilrenSql2, _connection))
        {
            var p1 = command.CreateParameter();
            p1.ParameterName = "@Id";
            p1.DbType = System.Data.DbType.Int32;
            p1.Value = 20;
            command.Parameters.Add(p1);

            using (var reader = command.ExecuteReader())
            {
                Blog blog = default!;
                var preId = 0;
                while (reader.Read())
                {
                    var arg0 = reader.GetInt32(0);
                    if (preId != arg0)
                    {
                        blog = blogDic[arg0];
                        preId = arg0;
                    }

                    int? arg2;
                    if (reader.IsDBNull(2))
                        arg2 = null;
                    else
                        arg2 = reader.GetInt32(2);

                    var post = new Post(reader.GetInt32(1), arg0, arg2, reader.GetDateTime(3), reader.GetString(4));
                    post.Blog = blog;
                    blog.Posts.Add(post);
                }
            }
        }
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }
}
