using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.SqlServer.Tests;

public class EndToEndTests
{
    readonly ILtConnection _connection;
    public EndToEndTests()
    {
        var provider = new ServiceProviderFactory().Create();
        _connection = provider.GetRequiredService<ILtConnection>();
    }

    static readonly Query<Blog> _selectQuery = Lt.Query<Blog>().ToImmutable();

    [Fact]
    public void Select()
    {
        var blogs = _connection.Select(_selectQuery);

        Assert.Equal(10000, blogs.Count);
        Assert.Equal(1, blogs[0].Id);
        Assert.Equal(11, blogs[10].Id);
    }

    static readonly Query<Blog> _selectWithWhereQuery = Lt.Query<Blog>().Where(_ => _.UserId < 5).ToImmutable();

    [Fact]
    public void Select_WithWhere()
    {
        var blogs = _connection.Select(_selectWithWhereQuery);

        Assert.Equal(3985, blogs.Count);
        Assert.Equal(4, blogs[0].Id);
        Assert.Equal(30, blogs[10].Id);
    }

    static readonly Query<Blog> _selectWithWhereAndOrderByQuery = Lt.Query<Blog>().Where(_ => _.UserId < 5).OrderBy(_ => _.User.Name).ToImmutable();

    [Fact]
    public void Select_WithWhereAndOrderBy()
    {
        var blogs = _connection.Select(_selectWithWhereAndOrderByQuery);

        Assert.Equal(3985, blogs.Count);
        Assert.Equal(15, blogs[0].Id);
        Assert.Equal(146, blogs[10].Id);
    }

    static readonly Query<Blog> _selectWithWParameterQuery = Lt.Query<Blog>().Where(_ => _.UserId < Lt.Arg<int>("UserId")).ToImmutable();

    [Fact]
    public void Select_WithParameter()
    {
        var blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 4 });

        Assert.Equal(3003, blogs.Count);
        blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 2 });
        Assert.Equal(980, blogs.Count);
    }

    static readonly Query<Blog> _selectWithChildrenHasParameterQuery = Lt.Query<Blog>().Where(_ => _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName"))).ToImmutable();

    [Fact]
    public void Select_WithChildrenHasParameter()
    {
        var blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "PLCJKJKRUK" });

        Assert.Equal(9938, blogs.Count);
        blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "GOFLFNVPAT" });
        Assert.Empty(blogs);
    }

    static readonly Query<Blog> _selectComplexQuery = Lt.Query<Blog>().Include(_ => _.User).Where(_ => _.CategoryId >= 4).Where(_ => _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName"))).Take("Take").ToImmutable();

    [Fact]
    public void Select_Complex()
    {
        var blogs = _connection.Select(_selectComplexQuery, new { Take = 2, UserName = "PLCJKJKRUK" });

        Assert.Equal(2, blogs.Count);
        Assert.Equal(100, blogs[0].Posts.Count);
        Assert.Equal(100, blogs[1].Posts.Count);
        blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "GOFLFNVPAT" });
        Assert.Empty(blogs);
    }

    static readonly Query<Category> _selectComplex2Query = Lt.Query<Category>()
        .Include(_ => _.Blogs).ThenInclude(_ => _.Posts).ThenInclude(_ => _.User)
        .Where(_ => _.Blogs.Any(_ => _.DateTime < Lt.Arg<DateTime>("DateTime") && _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName")))).Take("Take").ToImmutable();

    [Fact]
    public void Select_Complex2()
    {
        var categories = _connection.Select(_selectComplex2Query, new { Take = 2, DateTime = new DateTime(2010, 1, 1), UserName = "PLCJKJKRUK" });

        Assert.Equal(2, categories.Count);
        Assert.Equal(1035, categories[0].Blogs.Count);
        Assert.Equal(1016, categories[1].Blogs.Count);
    }

    static readonly Query<Post> _selectComplex3Query = Lt.Query<Post>()
        .Include(_ => _.User).ThenInclude(_ => _!.Blogs).ThenInclude(_ => _.User)
        .Where(_ => _.User!.Blogs.Any(_ => _.User.Name == Lt.Arg<string>("UserName"))).OrderBy(_ => _.Id).Skip("Skip").Take("Take").ToImmutable();

    [Fact]
    public void Select_Complex3()
    {
        var posts = _connection.Select(_selectComplex3Query, new { Skip = 10, Take = 5, UserName = "PLCJKJKRUK" });

        Assert.Equal(5, posts.Count);

        var user = posts[0].User;
        Assert.Equal(192, posts[0].Id);
        Assert.NotNull(user);
        Assert.Equal(10100, user.Blogs.Count);

        user = posts[2].User;
        Assert.Equal(226, posts[2].Id);
        Assert.NotNull(user);
        Assert.Equal(10100, user.Blogs.Count);
    }

    static readonly Query<Blog> _selectIncludeStringKeyTableQuery = Lt.Query<Blog>().Include(_ => _.User.Account).OrderBy(_ => _.Id).Take(5).ToImmutable();

    [Fact]
    public void Select_IncludeStringKeyTable()
    {
        var blogs = _connection.Select(_selectIncludeStringKeyTableQuery);

        Assert.Equal(5, blogs.Count);

        var account = blogs[3].User.Account;
        Assert.NotNull(account);
        Assert.Equal("ABOTBPQGHD", account.Password);

        account = blogs[2].User.Account;
        Assert.Null(account);
    }
}