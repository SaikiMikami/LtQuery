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

    readonly Query<Blog> _selectQuery = Lt.Query<Blog>().ToImmutable();

    [Fact]
    public void Select()
    {
        var blogs = _connection.Select(_selectQuery);

        Assert.Equal(10000, blogs.Count);
        Assert.Equal(1, blogs[0].Id);
        Assert.Equal(11, blogs[10].Id);
    }

    readonly Query<Blog> _selectWithWhereQuery = Lt.Query<Blog>().Where(_ => _.UserId < 5).ToImmutable();

    [Fact]
    public void SelectWithWhere()
    {
        var blogs = _connection.Select(_selectWithWhereQuery);

        Assert.Equal(4017, blogs.Count);
        Assert.Equal(5, blogs[0].Id);
        Assert.Equal(44, blogs[10].Id);
    }

    readonly Query<Blog> _selectWithWhereAndOrderByQuery = Lt.Query<Blog>().Where(_ => _.UserId < 5).OrderBy(_ => _.User.Name).ToImmutable();

    [Fact]
    public void SelectWithWhereAndOrderBy()
    {
        var blogs = _connection.Select(_selectWithWhereAndOrderByQuery);

        Assert.Equal(4017, blogs.Count);
        Assert.Equal(15, blogs[0].Id);
        Assert.Equal(143, blogs[10].Id);
    }

    readonly Query<Blog> _selectWithWParameterQuery = Lt.Query<Blog>().Where(_ => _.UserId < Lt.Arg<int>("UserId")).ToImmutable();

    [Fact]
    public void SelectWithParameter()
    {
        var blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 4 });

        Assert.Equal(3032, blogs.Count);
        blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 2 });
        Assert.Equal(1050, blogs.Count);
    }
}
