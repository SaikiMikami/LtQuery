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
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0", actual[0]);
        }

        [Fact]
        public void CreateSelectSql_WithOrderBy()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId]", actual[0]);
        }

        [Fact]
        public void CreateSelectSql_WithOrderByDescending()
        {
            var query = Lt.Query<Blog>().OrderByDescending(_ => _.CategoryId).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] DESC", actual[0]);
        }

        [Fact]
        public void CreateSelectSql_WithSkip()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Skip(10).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] OFFSET 10 ROWS", actual[0]);
        }

        [Fact]
        public void CreateSelectSql_WithTake()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Take(10).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT TOP (10) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId]", actual[0]);
        }

        [Fact]
        public void CreateSelectSql_WithSkipAndTake()
        {
            var query = Lt.Query<Blog>().OrderBy(_ => _.CategoryId).Skip(10).Take(5).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[CategoryId] OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[Id] = @Id", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithNotEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.Id != Lt.Arg<int>("Id")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[Id] != @Id", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithLessThan()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < @CategoryId", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithLessThanOrEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId <= Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] <= @CategoryId", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithGreaterThan()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId > Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] > @CategoryId", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithGreaterThanOrEqual()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId >= Lt.Arg<int>("CategoryId")).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] >= @CategoryId", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithAndAlso()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId == Lt.Arg<int>("CategoryId") && _.Id < 10).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] = @CategoryId AND t0.[Id] < 10", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithOrElse()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId == Lt.Arg<int>("CategoryId") || _.Id < 10).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] = @CategoryId OR t0.[Id] < 10", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithIncludeToReferenceOne()
        {
            var query = Lt.Query<Blog>().Include(_ => _.User).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Single(actual);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content], t1.[Id], t1.[Name], t1.[Email] FROM [Blog] AS t0 INNER JOIN [User] AS t1 ON t0.[UserId] = t1.[Id]", actual[0]);
        }

        [Fact]
        public void CreateSelectSqls_WithIncludeToOwnMany()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0", actual[0]);
            Assert.Equal("SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId]", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithWhere()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < 5).Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < 5", actual[0]);
            Assert.Equal("SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId] WHERE t0.[CategoryId] < 5", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithWhereAndParameter()
        {
            var query = Lt.Query<Blog>().Where(_ => _.CategoryId < Lt.Arg<int>("CategoryId")).Include(_ => _.Posts).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[CategoryId] < @CategoryId", actual[0]);
            Assert.Equal("SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId] WHERE t0.[CategoryId] < @CategoryId", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithOrderBy()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).OrderBy(_ => _.Title).ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[Title]", actual[0]);
            Assert.Equal("SELECT t0.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM [Blog] AS t0 INNER JOIN [Post] AS t1 ON t0.[Id] = t1.[BlogId] ORDER BY t0.[Title]", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithTake()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0", actual[0]);
            Assert.Equal("SELECT _.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT TOP (@Take) t0.[Id] FROM [Blog] AS t0) AS _ INNER JOIN [Post] AS t1 ON t1.[BlogId] = _.[Id]", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithTakeAndSkip()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).OrderBy(_ => _.Title).Skip("Skip").Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 ORDER BY t0.[Title] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", actual[0]);
            Assert.Equal("SELECT _.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT t0.[Id] FROM [Blog] AS t0 ORDER BY t0.[Title] OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY) AS _ INNER JOIN [Post] AS t1 ON t1.[BlogId] = _.[Id]", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithWhereAndTake()
        {
            var query = Lt.Query<Blog>().Include(_ => _.Posts).Where(_ => _.DateTime >= Lt.Arg<DateTime>("DateTime")).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime", actual[0]);
            Assert.Equal("SELECT _.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT TOP (@Take) t0.[Id] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime) AS _ INNER JOIN [Post] AS t1 ON t1.[BlogId] = _.[Id]", actual[1]);
        }

        [Fact]
        public void CreateSelectSqls_WithComplexInclude()
        {
            var query = Lt.Query<Blog>().Include(_ => _.User).Include(_ => _.Posts).Include(new[] { "Posts", "User" }).Where(_ => _.CategoryId >= 4).Where(_ => _.Posts.Any(_ => _.UserId == 1)).Take("Take").ToImmutable();
            var actual = _inst.CreateSelectSqls(query);
            Assert.Equal(2, actual.Count);

            Assert.Equal("SELECT TOP (@Take) t0.[Id], t0.[Title], t0.[CategoryId], t0.[UserId], t0.[DateTime], t0.[Content] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime", actual[0]);
            Assert.Equal("SELECT _.[Id], t1.[Id], t1.[BlogId], t1.[UserId], t1.[DateTime], t1.[Content] FROM (SELECT TOP (@Take) t0.[Id] FROM [Blog] AS t0 WHERE t0.[DateTime] >= @DateTime) AS _ INNER JOIN [Post] AS t1 ON t1.[BlogId] = _.[Id]", actual[1]);
        }
    }
}