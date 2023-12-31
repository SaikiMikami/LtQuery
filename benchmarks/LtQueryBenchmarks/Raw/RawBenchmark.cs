﻿using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace LtQueryBenchmarks.Raw;

class RawBenchmark : AbstractBenchmark
{
    SqlConnection _connection = default!;
    RandomEx _random = default!;
    public void Setup()
    {
        _connection = new SqlConnection(Constants.ConnectionString);
        _random = new(0);

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


    const string _selectIncludeChilrenSql = @"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] < @Id;
SELECT _.[Id], t2.[Id], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1 WHERE [Id] < @Id) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId];";

    public int SelectIncludeChilren()
    {
        var entities = new List<Blog>();
        var blogDic = new Dictionary<int, Blog>();

        using (var command = new SqlCommand(_selectIncludeChilrenSql, _connection))
        {
            var p1 = command.CreateParameter();
            p1.ParameterName = "@Id";
            p1.DbType = DbType.Int32;
            p1.Value = 20;
            command.Parameters.Add(p1);

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

    public async Task<int> SelectIncludeChilrenAsync()
    {
        var entities = new List<Blog>();
        var blogDic = new Dictionary<int, Blog>();

        using (var command = new SqlCommand(_selectIncludeChilrenSql, _connection))
        {
            var p = command.CreateParameter();
            p.ParameterName = "@Id";
            p.DbType = DbType.Int32;
            p.Value = 20;
            command.Parameters.Add(p);

            var reader = await command.ExecuteReaderAsync();
            try
            {
                Blog blog = default!;
                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt32(0);
                    blog = new Blog(id, reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                    entities.Add(blog);
                    blogDic.Add(id, blog);
                }

                await reader.NextResultAsync();

                var preId = 0;
                while (await reader.ReadAsync())
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
            finally
            {
                await reader.DisposeAsync();
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


    const string _selectComplexSql = @"
SELECT DISTINCT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId] 
FROM [Blog] AS t0 
INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id] 
INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] 
LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] 
WHERE t3.[Name] = @UserName 
ORDER BY t0.[Id] 
OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY; 

SELECT t0.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId] 
FROM (
  SELECT DISTINCT t0.[Id] 
  FROM [Blog] AS t0 
  INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] 
  LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] 
  WHERE t3.[Name] = @UserName 
  ORDER BY t0.[Id] 
  OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
) AS t0 
INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] 
LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]";

    public int SelectComplex()
    {
        var entities = new List<Blog>();

        using (var command = new SqlCommand(_selectComplexSql, _connection))
        {
            var p = command.CreateParameter();
            p.ParameterName = "@UserName";
            p.DbType = DbType.String;
            p.Value = "PLCJKJKRUK";
            command.Parameters.Add(p);
            p = command.CreateParameter();
            p.ParameterName = "@Skip";
            p.DbType = DbType.Int32;
            p.Value = 10;
            command.Parameters.Add(p);
            p = command.CreateParameter();
            p.ParameterName = "@Take";
            p.DbType = DbType.Int32;
            p.Value = 20;
            command.Parameters.Add(p);

            using (var reader = command.ExecuteReader())
            {
                var dictionary = new Dictionary<int, Blog>();
                Blog? blog = null;
                Dictionary<int, User> dictionary2 = new Dictionary<int, User>();
                User? user = null;
                int preId = 0;
                while (reader.Read())
                {
                    int @int = reader.GetInt32(0);
                    blog = new Blog(@int, reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetDateTime(4), reader.GetString(5));
                    entities.Add(blog);
                    dictionary.Add(@int, blog);
                    if (!reader.IsDBNull(6))
                    {
                        int int2 = reader.GetInt32(6);
                        if (preId != int2 && !dictionary2.TryGetValue(int2, out user))
                        {
                            user = new User(int2, reader.GetString(7), (!reader.IsDBNull(8)) ? reader.GetString(8) : null, (!reader.IsDBNull(9)) ? reader.GetString(9) : null);
                            dictionary2.Add(int2, user);
                        }
                        user!.Blogs.Add(blog);
                        blog.User = user;
                    }
                }
                reader.NextResult();
                int num3 = 0;
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    if (preId != id)
                    {
                        blog = dictionary[id];
                        preId = id;
                    }
                    Post post = new Post(reader.GetInt32(1), reader.GetInt32(2), (!reader.IsDBNull(3)) ? new int?(reader.GetInt32(3)) : default(int?), reader.GetDateTime(4), reader.GetString(5));
                    post.Blog = blog!;
                    blog!.Posts.Add(post);
                    if (!reader.IsDBNull(6))
                    {
                        int userId = reader.GetInt32(6);
                        if (num3 != userId && !dictionary2.TryGetValue(userId, out user))
                        {
                            user = new User(userId, reader.GetString(7), (!reader.IsDBNull(8)) ? reader.GetString(8) : null, (!reader.IsDBNull(9)) ? reader.GetString(9) : null);
                            dictionary2.Add(userId, user);
                        }
                        user!.Posts.Add(post);
                        post.User = user;
                    }
                }
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


    const string _addSql = @"
INSERT INTO [Tag] ([Name])
OUTPUT inserted.[Id] VALUES ";

    public int AddRange()
    {
        const int count = 10;

        var tags = new List<Tag>();
        for (var i = 0; i < count; i++)
        {
            tags.Add(new(_random.NextString()));
        }

        using (var tran = _connection.BeginTransaction())
        {
            var sqlb = new StringBuilder();
            sqlb.Append(@"INSERT INTO [Tag] ([Name]) OUTPUT inserted.[Id] VALUES ");
            for (var i = 0; i < count; i++)
            {
                if (i != 0)
                    sqlb.Append(", ");
                sqlb.Append("(@").Append(i).Append("_Name)");
            }
            using (var command = new SqlCommand(sqlb.ToString(), _connection))
            {
                for (var i = 0; i < count; i++)
                {
                    var p = command.CreateParameter();
                    p.ParameterName = $"@{i}_Name";
                    p.DbType = DbType.String;
                    command.Parameters.Add(p);
                }
                command.Transaction = tran;
                for (var i = 0; i < 10; i++)
                {
                    command.Parameters[i].Value = tags[i].Name;
                }

                using (var reader = command.ExecuteReader())
                {
                    var i = 0;
                    while (reader.Read())
                    {
                        tags[i].Id = reader.GetInt32(0);
                    }
                }
            }
            tran.Commit();
        }
        return 0;
    }
}
