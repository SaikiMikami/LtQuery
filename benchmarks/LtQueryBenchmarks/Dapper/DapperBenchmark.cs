using Dapper;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace LtQueryBenchmarks.Dapper;

public class DapperBenchmark : AbstractBenchmark
{

    IDbConnection _connection = default!;
    RandomEx _random = default!;
    public void Setup()
    {
        _connection = new SqlConnection(Constants.ConnectionString);
        _random = new(0);

        _connection.Open();

        _connection.Query<Blog>(_singleSql);
        _connection.Query<Blog>(_selectSimpleSql);

        using (var multi = _connection.QueryMultiple(_selectIncludeChilrenSql, new { Id = 20 }))
        {
            var entities = multi.Read<Blog>().ToArray();
            var posts = multi.Read<Post>().ToArray();
        }
    }

    public void Cleanup()
    {
        _connection.Dispose();
    }

    const string _singleSql = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] = 1";

    public int SelectSingle()
    {
        var entity = _connection.QuerySingle<Blog>(_singleSql);

        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }

    const string _selectSimpleSql = "SELECT TOP(20) [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] ORDER BY [Id]";

    public int SelectSimple()
    {

        var entities = _connection.Query<Blog>(_selectSimpleSql).ToArray();

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }


    const string _selectIncludeChilrenSql = @"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] < @Id;
SELECT t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1 WHERE [Id] < @Id) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId];";

    public int SelectIncludeChilren()
    {

        IReadOnlyList<Blog> entities;
        using (var multi = _connection.QueryMultiple(_selectIncludeChilrenSql, new { Id = 20 }))
        {
            entities = multi.Read<Blog>().ToArray();
            var posts = multi.Read<Post>();

            var blogDic = entities.ToDictionary(_ => _.Id);
            foreach (var post in posts)
            {
                var blog = blogDic[post.BlogId];
                blog.Posts.Add(post);
                post.Blog = blog;
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
        IReadOnlyList<Blog> entities;
        using (var multi = await _connection.QueryMultipleAsync(_selectIncludeChilrenSql, new { Id = 20 }))
        {
            entities = (await multi.ReadAsync<Blog>()).ToArray();
            var posts = await multi.ReadAsync<Post>();

            var blogDic = entities.ToDictionary(_ => _.Id);
            foreach (var post in posts)
            {
                var blog = blogDic[post.BlogId];
                blog.Posts.Add(post);
                post.Blog = blog;
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
LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
";

    public int SelectComplex()
    {
        var accum = 0;

        IReadOnlyList<Blog> entities;
        using (var multi = _connection.QueryMultiple(_selectComplexSql, new { UserName = "PLCJKJKRUK", Skip = 20, Take = 20 }))
        {
            var userDic = new Dictionary<int, User>();
            entities = multi.Read<Blog, User, Blog>((b, u) =>
            {
                if (!userDic.TryGetValue(u.Id, out var user))
                {
                    userDic.Add(u.Id, u);
                    user = u;
                }
                b.User = user;
                user.Blogs.Add(b);
                return b;
            }).ToArray();
            var posts = multi.Read<Post, User?, Post>((p, u) =>
            {
                if (u != null)
                {
                    if (!userDic.TryGetValue(u.Id, out var user))
                    {
                        userDic.Add(u.Id, u);
                        user = u;
                    }
                    p.User = user;
                    user.Posts.Add(p);
                }
                return p;
            });

            var blogDic = entities.ToDictionary(_ => _.Id);
            foreach (var post in posts)
            {
                var blog = blogDic[post.BlogId];
                blog.Posts.Add(post);
                post.Blog = blog;
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
                sqlb.Append("(@_").Append(i).Append("_Name)");
            }
            var ids = _connection.Query<int>(sqlb.ToString(), new
            {
                _0_Name = _random.NextString(),
                _1_Name = _random.NextString(),
                _2_Name = _random.NextString(),
                _3_Name = _random.NextString(),
                _4_Name = _random.NextString(),
                _5_Name = _random.NextString(),
                _6_Name = _random.NextString(),
                _7_Name = _random.NextString(),
                _8_Name = _random.NextString(),
                _9_Name = _random.NextString(),
            }, tran);
            {
                var i = 0;
                foreach (var id in ids)
                {
                    tags[i++].Id = id;
                }
            }

            tran.Commit();
        }
        return 0;
    }
}
