using LtQuery.Metadata;
using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.SqlServer.Tests
{
    public class SqlBuilderTests
    {
        IServiceProvider _provider;
        SqlBuilder _inst;
        public SqlBuilderTests()
        {
            _provider = new ServiceProviderFactory().Create();
            var metaService = _provider.GetRequiredService<EntityMetaService>();
            _inst = new SqlBuilder(metaService);
        }

        [Fact]
        public void CreateSelectSql()
        {
            var query = Lt.Query<Blog>().ToImmutable();
            var actual = _inst.CreateSelectSql(query);
            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0", actual);
        }

        [Fact]
        public void CreateSelectSql_WithOrderBy()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithOrderByDescending()
        {
            var query = Lt.Query<Blog>().OrderByDescending(_ => _.CategoryId).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] DESC", actual);
        }

        [Fact]
        public void CreateSelectSql_WithSkip()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Skip(10).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] OFFSET 10 ROWS", actual);
        }

        [Fact]
        public void CreateSelectSql_WithTake()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Take(10).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT TOP (10) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithSkipAndTake()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Skip(10).Take(5).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY", actual);
        }

        [Fact]
        public void CreateSelectSql_WithEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[Id] = @Id", actual);
        }

        [Fact]
        public void CreateSelectSql_WithNotEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.Id != Lt.Arg<int>("Id")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[Id] != @Id", actual);
        }

        [Fact]
        public void CreateSelectSql_WithLessThan()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < @CategoryId", actual);
        }

        [Fact]
        public void CreateSelectSql_WithLessThanOrEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId <= Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] <= @CategoryId", actual);
        }

        [Fact]
        public void CreateSelectSql_WithGreaterThan()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId > Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] > @CategoryId", actual);
        }

        [Fact]
        public void CreateSelectSql_WithGreaterThanOrEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId >= Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] >= @CategoryId", actual);
        }

        [Fact]
        public void CreateSelectSql_WithAndAlso()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId == Lt.Arg<int>("CategoryId") && _.Id < 10).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] = @CategoryId AND t0.[Id] < 10", actual);
        }

        [Fact]
        public void CreateSelectSql_WithOrElse()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId == Lt.Arg<int>("CategoryId") || _.Id < 10).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] = @CategoryId OR t0.[Id] < 10", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeToReferenceOne()
        {
            var query = Lt.Query<Blog>().Include(_ => _.User).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId] FROM [Blog] AS t0 INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAnd()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndWhere()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < 5).Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < 5; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT t0.[Id] FROM [Blog] AS t0 WHERE t0.[CategoryId] < 5) AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndWhereAndParameter()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < Lt.Arg<int>("CategoryId")).Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < @CategoryId; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT t0.[Id] FROM [Blog] AS t0 WHERE t0.[CategoryId] < @CategoryId) AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndOrderBy()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).OrderBy(_ => _.Title).ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[Title]; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId] ORDER BY t0.[Title]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndTake()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT TOP (@Take) t0.[Id] FROM [Blog] AS t0) AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndTakeAndSkip()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).OrderBy(_ => _.Title).Skip("Skip").Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[Title] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT t0.[Id] FROM [Blog] AS t0 ORDER BY t0.[Title] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY) AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndWhereAndTake()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).Where(_ => _.DateTime >= Lt.Arg<DateTime>("DateTime")).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime; SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT TOP (@Take) t0.[Id] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime) AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual);
        }

        /*
         * SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId]
         * FROM [Blog] AS t0
         * INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]
         * INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId]
         * LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         * WHERE t0.[CategoryId] >= 4 AND t3.[Name] = @UserName;
         * 
         * SELECT t0.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId]
         * FROM (
         *   SELECT DISTINCT TOP (@Take) t0.[Id]
         *   FROM [Blog] AS t0
         *   INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]
         *   INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId]
         *   LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         *   WHERE t0.[CategoryId] >= 4 AND t3.[Name] = @UserName) AS t0
         * INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId]
         * LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         */
        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndComplexInclude()
        {
            var query = Lt.Query<Blog>().Include(_ => _.User).Where(_ => _.CategoryId >= 4).Where(_ => _.Posts.Any(_ => _.User.Name == Lt.Arg<string>("UserName"))).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId] FROM [Blog] AS t0 INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id] INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t0.[CategoryId] >= 4 AND t3.[Name] = @UserName; SELECT t0.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId] FROM (SELECT DISTINCT TOP (@Take) t0.[Id] FROM [Blog] AS t0 INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id] INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t0.[CategoryId] >= 4 AND t3.[Name] = @UserName) AS t0 INNER JOIN [Post] AS t2 ON t0.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]", actual);
        }

        /*
         * SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Name]
         * FROM [Category] AS t0
         * INNER JOIN [Blog] AS t1 ON t0.[Id] = t2.[CategoryId]
         * INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId]
         * LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         * WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName;
         * 
         * SELECT t0.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id]
         * FROM (
         *   SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Name]
         *   FROM [Category] AS t0
         *   INNER JOIN [Blog] AS t1 ON t0.[Id] = t2.[CategoryId]
         *   INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId]
         *   LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         *   WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName;
         * ) AS t0
         * INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId];
         * 
         * SELECT t1.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId]
         * FROM (
         *   SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Name]
         *   FROM [Category] AS t0
         *   INNER JOIN [Blog] AS t1 ON t0.[Id] = t2.[CategoryId]
         *   INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId]
         *   LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         *   WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName;
         * ) AS t0
         * INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId]
         * INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId]
         * LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         * 
         */
        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndComplexInclude2()
        {
            var query = Lt.Query<Category>().Include(new[] { "Blogs", "Posts", "User" }).Where(_ => _.Blogs.Any(_ => _.DateTime < Lt.Arg<DateTime>("DateTime") && _.Posts.Any(_ => _.User.Name == Lt.Arg<string>("UserName")))).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT DISTINCT TOP (@Take) t0.[Id], t0.[Name] FROM [Category] AS t0 INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId] INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName; SELECT t0.[Id], t1.[Id], t1.[Title], t1.[CategoryId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT DISTINCT TOP (@Take) t0.[Id] FROM [Category] AS t0 INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId] INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName) AS t0 INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId]; SELECT t1.[Id], t2.[Id], t2.[BlogId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId] FROM (SELECT DISTINCT TOP (@Take) t0.[Id] FROM [Category] AS t0 INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId] INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t1.[DateTime] < @DateTime AND t3.[Name] = @UserName) AS t0 INNER JOIN [Blog] AS t1 ON t0.[Id] = t1.[CategoryId] INNER JOIN [Post] AS t2 ON t1.[Id] = t2.[BlogId] LEFT JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]", actual);
        }

        /*
         * SELECT DISTINCT t0.[Id], t0.[BlogId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId]
         * FROM [Post] AS t0
         * LEFT JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]
         * INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId]
         * INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         * ORDER BY t0.[Id]
         * WHERE t3.[Name] = @UserName OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
         * 
         * SELECT t1.[Id], t2.[Id], t2.[Title], t2.[CategoryId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId]
         * FROM (
         *   SELECT DISTINCT t1.[Id], t0.[Id] AS _sort1
         *   FROM [Post] AS t0
         *   LEFT JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]
         *   INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId]
         *   INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         *   WHERE t3.[Name] = @UserName
         *   ORDER BY t0.[Id]
         *   OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
         * ) AS t0
         * INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId]
         * INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]
         */
        [Fact]
        public void CreateSelectSql_WithIncludeChildrenAndComplexInclude3()
        {
            var query = Lt.Query<Post>().Include(new[] { "User", "Blogs", "User" }).Where(_ => _.User.Blogs.Any(_ => _.User.Name == Lt.Arg<string>("UserName"))).OrderBy(_ => _.Id).Skip("Skip").Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSql(query);

            Assert.Equal("SELECT DISTINCT t0.[Id], t0.[BlogId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email], t1.[AccountId] FROM [Post] AS t0 LEFT JOIN [User] AS t1 ON t0.[UserId] = t1.[Id] INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId] INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t3.[Name] = @UserName ORDER BY t0.[Id] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY; SELECT t1.[Id], t2.[Id], t2.[Title], t2.[CategoryId], t2.[UserId], t2.[DateTime], t2.[Content], t3.[Id], t3.[Name], t3.[Email], t3.[AccountId] FROM (SELECT DISTINCT t1.[Id], t0.[Id] AS _sort1 FROM [Post] AS t0 LEFT JOIN [User] AS t1 ON t0.[UserId] = t1.[Id] INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId] INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id] WHERE t3.[Name] = @UserName ORDER BY t0.[Id] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY) AS t1 INNER JOIN [Blog] AS t2 ON t1.[Id] = t2.[UserId] INNER JOIN [User] AS t3 ON t2.[UserId] = t3.[Id]", actual);
        }
    }
}