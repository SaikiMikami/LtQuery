using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LtQueryBenchmarks.Raw;

class RawBenchmark : AbstractBenchmark
{
    SqlConnection _connection = default!;

    const string _singleSql = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] = 1";
    SqlCommand _singleCommand = default!;

    const string _selectSimpleSql = "SELECT TOP(20) [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] ORDER BY [Id]";
    SqlCommand _selectSimpleCommand = default!;

    const string _selectIncludeChilrenSql = @"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] < @Id;
SELECT _.[Id], t2.[Id], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1 WHERE [Id] < @Id) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId];";
    SqlCommand _selectIncludeChilrenCommand = default!;

    public void Setup()
    {
        _connection = new SqlConnection(Constants.ConnectionString);
        _connection.Open();

        _singleCommand = new SqlCommand(_singleSql, _connection);
        _selectSimpleCommand = new SqlCommand(_selectSimpleSql, _connection);

        _selectIncludeChilrenCommand = new SqlCommand(_selectIncludeChilrenSql, _connection);
        var p1 = _selectIncludeChilrenCommand.CreateParameter();
        p1.ParameterName = "@Id";
        p1.DbType = DbType.Int32;
        _selectIncludeChilrenCommand.Parameters.Add(p1);
    }

    public void Cleanup()
    {
        _singleCommand.Dispose();
        _selectSimpleCommand.Dispose();
        _selectIncludeChilrenCommand.Dispose();
        _connection.Dispose();
    }

    public int SelectSingle()
    {
        Blog entity;
        using (var reader = _singleCommand.ExecuteReader())
        {
            if (!reader.Read())
                throw new InvalidOperationException("not data");

            entity = new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));

            if (reader.Read())
                throw new InvalidOperationException("multi");
        }
        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }

    public int SelectSimple()
    {
        var entities = new List<Blog>();
        using (var reader = _selectSimpleCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                entities.Add(new(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5)));
            }
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }


    public int SelectIncludeChilren()
    {
        var entities = new List<Blog>();
        var blogDic = new Dictionary<int, Blog>();

        var command = _selectIncludeChilrenCommand;
        command.Parameters[0].Value = 20;

        using (var reader = command.ExecuteReader())
        {
            Blog blog = default!;
            while (reader.Read())
            {
                var id = reader.GetInt32(0);
                blog = new Blog(id, reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                entities.Add(blog);
                blogDic.Add(id, blog);
            }

            reader.NextResult();

            var preId = 0;
            while (reader.Read())
            {
                var arg0 = reader.GetInt32(0);
                if (preId != arg0)
                {
                    blog = blogDic[arg0];
                    preId = arg0;
                }

                var post = new Post(reader.GetInt32(1), arg0, (!reader.IsDBNull(2)) ? new int?(reader.GetInt32(2)) : default(int?), reader.GetDateTime(3), reader.GetString(4));
                post.Blog = blog;
                blog.Posts.Add(post);
            }
        }

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
