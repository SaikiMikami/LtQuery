using Dapper;
using LtQuery.TestData;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LtQueryBenchmarks.Dapper
{
    public class DapperBenchmark : AbstractBenchmark
    {

        IDbConnection _connection;
        public void Setup()
        {
            _connection = new SqlConnection(Constants.ConnectionString);

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
            var accum = 0;

            var entities = _connection.Query<Blog>(_selectSimpleSql).ToArray();

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
