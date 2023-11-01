using Dapper;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LtQueryBenchmarks.Dapper
{
    public class DapperBenchmark : AbstractBenchmark
    {
        const string _singleSql = "SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] = 1";
        const string _selectSimpleSql = "SELECT TOP(20) [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] ORDER BY [Id]";
        const string _allIncludeUniqueManySql = @"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog];
SELECT t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content] FROM (SElECT t1.[Id] FROM [Blog] AS t1) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId];
";

        IDbConnection _connection;
        public void Setup()
        {
            _connection = new SqlConnection(Constants.ConnectionString);

            _connection.Query<Blog>(_singleSql);
            _connection.Query<Blog>(_selectSimpleSql);


            using (var multi = _connection.QueryMultiple(_allIncludeUniqueManySql))
            {
                var entities = multi.Read<Blog>().ToArray();
                var posts = multi.Read<Post>().ToArray();
            }

        }

        public void Cleanup()
        {
            _connection.Dispose();
        }


        public int SelectSingle()
        {
            var entity = _connection.QuerySingle<Blog>(_singleSql);

            var accum = 0;
            AddHashCode(ref accum, entity.Id);
            return accum;
        }

        public int SelectSimple()
        {
            var accum = 0;

            var entities = _connection.Query<Blog>(_selectSimpleSql).ToArray();

            foreach (var entity in entities)
            {
                AddHashCode(ref accum, entity.Id);
            }
            return accum;
        }

        public int SelectAllIncludeUniqueMany()
        {
            var accum = 0;
            var dict = new Dictionary<int, Post>();

            //var entities = _connection.Query<_3_A, _3_B, _3_A>(_allIncludeUniqueManySql,
            //    (a, b) =>
            //    {
            //        a.Bs.Add(b);
            //        return a;
            //    });

            IReadOnlyList<Blog> entities;
            using (var multi = _connection.QueryMultiple(_allIncludeUniqueManySql))
            {
                entities = multi.Read<Blog>().ToArray();
                var posts = multi.Read<Post>().ToArray();

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
            }
            return accum;
        }



        const string _selectIncludeChilrenSql = @"
SELECT [Id], [Title], [CategoryId], [UserId], [DateTime], [Content] FROM [Blog] WHERE [Id] < @Id;
SELECT t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content]  FROM (SElECT t1.[Id] FROM [Blog] AS t1 WHERE [Id] < @Id) AS _ INNER JOIN [Post] AS t2 ON _.[Id] = t2.[BlogId];";

        public int SelectIncludeChilren()
        {
            var accum = 0;

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

            foreach (var entity in entities)
            {
                AddHashCode(ref accum, entity.Id);
                foreach (var post in entity.Posts)
                    AddHashCode(ref accum, post.Id);
            }
            return accum;
        }
    }
}
