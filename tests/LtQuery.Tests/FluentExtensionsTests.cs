using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;

namespace LtQuery.Tests;

public class FluentExtensionsTests
{
    class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public int CategoryId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string Content { get; set; } = default!;

        public User User { get; set; } = default!;
        public List<Post> Posts { get; } = new();
    }
    class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public string Content { get; set; } = default!;

        public Blog Blog { get; set; } = default!;
        public User User { get; set; } = default!;
    }
    class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public int ZoneId { get; set; }

        public Zone Zone { get; set; } = new();
        public List<Blog> Blogs { get; set; } = new();
        public List<Post> Posts { get; set; } = new();
    }
    class Zone
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
    }

    [Fact]
    public void Skip_WithIntConstant()
    {
        var query = Lt.Query<Blog>().Skip(2).ToImmutable();

        var v = query.SkipCount as ConstantValue;
        Assert.NotNull(v);
        Assert.Equal("2", v.Value);
    }

    [Fact]
    public void Skip_WithIntConstant2()
    {
        var query = Lt.Query<Blog>().Skip(_ => 2).ToImmutable();

        var v = query.SkipCount as ConstantValue;
        Assert.NotNull(v);
        Assert.Equal("2", v.Value);
    }
    [Fact]
    public void Skip_WithParameter()
    {
        const string name = "Skip1";
        var query = Lt.Query<Blog>().Skip(_ => Lt.Arg<int>(name)).ToImmutable();

        var v = query.SkipCount as ParameterValue;
        Assert.NotNull(v);
        Assert.Equal(name, v.Name);

    }

    [Fact]
    public void Take_WithIntConstant()
    {
        var query = Lt.Query<Blog>().Take(4).ToImmutable();

        var v = query.TakeCount as ConstantValue;
        Assert.NotNull(v);
        Assert.Equal("4", v.Value);
    }
    [Fact]
    public void Take_WithIntConstant2()
    {
        var query = Lt.Query<Blog>().Take(_ => 4).ToImmutable();

        var v = query.TakeCount as ConstantValue;
        Assert.NotNull(v);
        Assert.Equal("4", v.Value);
    }
    [Fact]
    public void Take_WithParameter()
    {
        const string name = "Take1";
        var query = Lt.Query<Blog>().Take(_ => Lt.Arg<int>(name)).ToImmutable();

        var v = query.TakeCount as ParameterValue;
        Assert.NotNull(v);
        Assert.Equal(name, v.Name);
    }

    [Fact]
    public void OrderBy()
    {
        var query = Lt.Query<Blog>().OrderBy(_ => _.User.Name).ToImmutable();

        var orderBy = query.OrderBys[0];
        Assert.Equal(OrderByType.Asc, orderBy.Type);
        Assert.Equal("Name", orderBy.Property.Name);
        Assert.Equal("User", orderBy.Property.Parent!.Name);
    }

    [Fact]
    public void OrderByDescending()
    {
        var query = Lt.Query<Blog>().OrderByDescending(_ => _.User.Name).ToImmutable();

        var orderBy = query.OrderBys[0];
        Assert.Equal(OrderByType.Desc, orderBy.Type);
        Assert.Equal("Name", orderBy.Property.Name);
        Assert.Equal("User", orderBy.Property.Parent!.Name);
    }

    [Fact]
    public void Include()
    {
        var query = Lt.Query<Blog>().Include(_ => _.User.Zone).ToImmutable();

        var a = query.Includes;
        Assert.Equal("User", a[0].PropertyName);
        Assert.Equal("Zone", a[0].Includes[0].PropertyName);
    }

    [Fact]
    public void Where()
    {
        var query = Lt.Query<Blog>().Where(_ => _.User.Name == Lt.Arg<string>("Name")).ToImmutable();

        var equal = query.Condition as EqualOperator;
        Assert.NotNull(equal);

        var lhs = equal.Lhs as PropertyValue;
        Assert.NotNull(lhs);
        Assert.Equal("Name", lhs.Name);
        if (lhs.Parent == null)
            throw new Exception();
        Assert.Equal("User", lhs.Parent.Name);
        Assert.Null(lhs.Parent.Parent);

        var rhs = equal.Rhs as ParameterValue;
        Assert.NotNull(rhs);
        Assert.Equal("Name", rhs.Name);
        Assert.Equal(typeof(string), rhs.Type);
    }

    [Fact]
    public void Where_IncludedAnyMethod()
    {
        var query = Lt.Query<Blog>().Where(_ => _.Posts.Any(_ => _.User.Name == Lt.Arg<string>("Name"))).ToImmutable();

        var equal = query.Condition as EqualOperator;
        Assert.NotNull(equal);

        var lhs = equal.Lhs as PropertyValue;
        Assert.NotNull(lhs);
        Assert.Equal("Name", lhs.Name);
        if (lhs.Parent == null)
            throw new Exception();
        Assert.Equal("User", lhs.Parent.Name);
        if (lhs.Parent.Parent == null)
            throw new Exception();
        Assert.Equal("Posts", lhs.Parent.Parent.Name);
        Assert.Null(lhs.Parent.Parent.Parent);

        var rhs = equal.Rhs as ParameterValue;
        Assert.NotNull(rhs);
        Assert.Equal("Name", rhs.Name);
        Assert.Equal(typeof(string), rhs.Type);
    }
}