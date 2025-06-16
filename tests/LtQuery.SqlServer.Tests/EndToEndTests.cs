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

        Assert.Equal(3983, blogs.Count);
        Assert.Equal(3, blogs[0].Id);
        Assert.Equal(30, blogs[10].Id);
    }

    static readonly Query<Blog> _selectWithWhereAndOrderByQuery = Lt.Query<Blog>().Where(_ => _.UserId < 5).OrderBy(_ => _.User.Name).ToImmutable();

    [Fact]
    public void Select_WithWhereAndOrderBy()
    {
        var blogs = _connection.Select(_selectWithWhereAndOrderByQuery);

        Assert.Equal(3983, blogs.Count);
        Assert.Equal(3, blogs[0].Id);
        Assert.Equal(64, blogs[10].Id);
    }

    static readonly Query<Blog> _selectWithWParameterQuery = Lt.Query<Blog>().Where(_ => _.UserId < Lt.Arg<int>("UserId")).ToImmutable();

    [Fact]
    public void Select_WithParameter()
    {
        var blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 4 });

        Assert.Equal(3004, blogs.Count);
        blogs = _connection.Select(_selectWithWParameterQuery, new { UserId = 2 });
        Assert.Equal(977, blogs.Count);
    }

    static readonly Query<Blog> _selectWithChildrenHasParameterQuery = Lt.Query<Blog>().Where(_ => _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName"))).ToImmutable();

    [Fact]
    public void Select_WithChildrenHasParameter()
    {
        var blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "PLCJKJKRUK" });

        Assert.Equal(9953, blogs.Count);
        blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "OIAYBTKOBN" });
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
        blogs = _connection.Select(_selectWithChildrenHasParameterQuery, new { UserName = "OIAYBTKOBN" });
        Assert.Empty(blogs);
    }

    static readonly Query<Category> _selectComplex2Query = Lt.Query<Category>()
        .Include(_ => _.Blogs).ThenInclude(_ => _.Posts).ThenInclude(_ => _.User)
        .Where(_ => _.Blogs.Any(_ => _.DateTime < Lt.Arg<DateTime>("DateTime") && _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName")))).OrderBy(_ => _.Id).Take("Take").ToImmutable();

    [Fact]
    public void Select_Complex2()
    {
        var categories = _connection.Select(_selectComplex2Query, new { Take = 2, DateTime = new DateTime(2010, 1, 1), UserName = "PLCJKJKRUK" });

        Assert.Equal(2, categories.Count);
        Assert.Equal(984, categories[0].Blogs.Count);
        Assert.Equal(969, categories[1].Blogs.Count);
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
        Assert.Equal(204, posts[0].Id);
        Assert.NotNull(user);
        Assert.Equal(9560, user.Blogs.Count);

        user = posts[2].User;
        Assert.Equal(318, posts[2].Id);
        Assert.NotNull(user);
        Assert.Equal(9560, user.Blogs.Count);
    }

    static readonly Query<Blog> _selectIncludeStringKeyTableQuery = Lt.Query<Blog>().Include(_ => _.User.Account).OrderBy(_ => _.Id).Take(5).ToImmutable();

    [Fact]
    public void Select_IncludeStringKeyTable()
    {
        var blogs = _connection.Select(_selectIncludeStringKeyTableQuery);

        Assert.Equal(5, blogs.Count);

        var account = blogs[3].User.Account;
        Assert.NotNull(account);
        Assert.Equal("XQXPWOVBTP", account.Password);

        account = blogs[2].User.Account;
        Assert.Null(account);
    }

    static readonly Query<Blog> _selectIncludeManyToManyQuery = Lt.Query<Blog>().Include(_ => _.BlogTags).ThenInclude(_ => _.Tag).OrderBy(_ => _.Id).Take(5).ToImmutable();

    [Fact]
    public void Select_IncludeManyToMany()
    {
        var blogs = _connection.Select(_selectIncludeManyToManyQuery);

        Assert.Equal(5, blogs.Count);

        var tags = blogs[3].BlogTags.Select(_ => _.Tag).ToArray();
        Assert.Equal("LRKAPDXIFL", tags[1].Name);
    }

    [Fact]
    public async Task SelectAsync()
    {
        var blogs = await _connection.SelectAsync(_selectQuery);

        Assert.Equal(10000, blogs.Count);
        Assert.Equal(1, blogs[0].Id);
        Assert.Equal(11, blogs[10].Id);
    }
    [Fact]
    public async Task SelectAsync_Complex2()
    {
        var categories = await _connection.SelectAsync(_selectComplex2Query, new { Take = 2, DateTime = new DateTime(2010, 1, 1), UserName = "PLCJKJKRUK" });

        Assert.Equal(2, categories.Count);
        Assert.Equal(984, categories[0].Blogs.Count);
        Assert.Equal(969, categories[1].Blogs.Count);
    }

    static readonly Query<Blog> _signleQuery = Lt.Query<Blog>().Where(_ => _.Id == 10).ToImmutable();

    [Fact]
    public void Single()
    {
        var blog = _connection.Single(_signleQuery);

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    [Fact]
    public async Task SingleAsync()
    {
        var blog = await _connection.SingleAsync(_signleQuery);

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    static readonly Query<Blog> _signleWithParameterQuery = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void Single_WithParameter()
    {
        var blog = _connection.Single(_signleWithParameterQuery, new { Id = 10 });

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    [Fact]
    public async Task SingleAsync_WithParameter()
    {
        var blog = await _connection.SingleAsync(_signleWithParameterQuery, new { Id = 10 });

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    static readonly Query<Blog> _firstQuery = Lt.Query<Blog>().Where(_ => _.Id == 10).ToImmutable();

    [Fact]
    public void First()
    {
        var blog = _connection.First(_firstQuery);

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    [Fact]
    public async Task FirstAsync()
    {
        var blog = await _connection.FirstAsync(_firstQuery);

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    static readonly Query<Blog> _firstWithParameterQuery = Lt.Query<Blog>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void First_WithParameter()
    {
        var blog = _connection.First(_firstWithParameterQuery, new { Id = 10 });

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    [Fact]
    public async Task FirstAsync_WithParameter()
    {
        var blog = await _connection.FirstAsync(_firstWithParameterQuery, new { Id = 10 });

        Assert.Equal(10, blog.Id);
        Assert.Equal("HOURLWWOYG", blog.Title);
    }

    static readonly Query<Blog> _countQuery = Lt.Query<Blog>().ToImmutable();

    [Fact]
    public void Count()
    {
        var count = _connection.Count(_countQuery);
        Assert.Equal(10000, count);
    }

    static readonly Query<Blog> _countWithParameterQuery = Lt.Query<Blog>().Where(_ => _.Id < Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void Count_WithParameter()
    {
        var count = _connection.Count(_countWithParameterQuery, new { Id = 5000 });
        Assert.Equal(4999, count);
    }

    [Fact]
    public async Task CountAsync()
    {
        var count = await _connection.CountAsync(_countQuery);
        Assert.Equal(10000, count);
    }

    [Fact]
    public async Task CountAsync_WithParameter()
    {
        var count = await _connection.CountAsync(_countWithParameterQuery, new { Id = 5000 });
        Assert.Equal(4999, count);
    }

    static readonly Query<User> _getUser = Lt.Query<User>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void Rollback()
    {
        int id;
        User user;
        using (var transaction = _connection.BeginTransaction())
        {
            user = new User("name", "email", null);
            Assert.Equal(0, user.Id);

            _connection.Add(user);

            id = user.Id;
            Assert.NotEqual(0, id);

            user = _connection.Single(_getUser, new { Id = id });
            Assert.NotNull(user);
            Assert.Equal("name", user.Name);
            Assert.Equal("email", user.Email);
            Assert.Null(user.AccountId);


            transaction.Rollback();
        }

        try
        {
            user = _connection.Single(_getUser, new { Id = id });
            Assert.Fail();
        }
        catch { }
    }

    [Fact]
    public void Add_WithChild()
    {
        var random = new RandomEx(0);
        using (var tran = _connection.BeginTransaction())
        using (var unitOfWork = _connection.CreateUnitOfWork())
        {
            var user = new User(random.NextString(), null, null);
            unitOfWork.Add(user);
            var category = new Category(random.NextString());
            unitOfWork.Add(category);
            var blog = new Blog(random.NextString(), category, user, random.NextDateTime(), random.NextString());
            unitOfWork.Add(blog);

            unitOfWork.Commit();

            Assert.NotEqual(0, user.Id);
            Assert.NotEqual(0, category.Id);
            Assert.NotEqual(0, blog.Id);
            Assert.Equal(user.Id, blog.UserId);
            Assert.Equal(category.Id, blog.CategoryId);

            var user2 = _connection.Single(Lt.Query<User>().Where(_ => _.Id == Lt.Arg<int>("Id")), new { Id = user.Id });
            Assert.Equal(user.Name, user2.Name);
        }
    }

    [Fact]
    public void Add_Many()
    {
        var random = new RandomEx(0);
        using (var transaction = _connection.BeginTransaction())
        {
            var users = new List<User>();
            for (var i = 0; i < 1000; i++)
            {
                users.Add(new(random.NextString(), null, null));
            }

            _connection.AddRange(users);

            Assert.NotEqual(0, users[0].Id);
            Assert.Equal(users[0].Id + 1, users[1].Id);
            Assert.Equal(users[0].Id + 2, users[2].Id);

            var user2 = _connection.Single(Lt.Query<User>().Where(_ => _.Id == Lt.Arg<int>("Id")), new { Id = users[0].Id });
            Assert.Equal(users[0].Name, user2.Name);
        }
    }

    static readonly Query<Blog> _getBlogWithTagQuery = Lt.Query<Blog>().Include(_ => _.BlogTags).ThenInclude(_ => _.Tag).Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void Add_MultiPrimaryKeyTable()
    {
        var random = new RandomEx(0);
        using (var transaction = _connection.BeginTransaction())
        {
            var tag = new BlogTag(1, 5);

            _connection.Add(tag);

            var blog = _connection.Single(_getBlogWithTagQuery, new { Id = 1 });

            Assert.Contains(blog.BlogTags, _ => _.Tag.Id == 5);

        }
    }

    [Fact]
    public void Upddate_Many()
    {
        var random = new RandomEx(0);
        using (var transaction = _connection.BeginTransaction())
        {
            var users = new List<User>();
            for (var i = 0; i < 1000; i++)
            {
                users.Add(new(i + 1, random.NextString(), null, null));
            }

            _connection.UpdateRange(users);

            var user2 = _connection.Single(Lt.Query<User>().Where(_ => _.Id == Lt.Arg<int>("Id")), new { Id = users[0].Id });
            Assert.Equal(users[0].Name, user2.Name);
        }
    }

    [Fact]
    public void Remove_Many()
    {
        var random = new RandomEx(0);
        using (var transaction = _connection.BeginTransaction())
        {
            var postss = new List<Post>();
            for (var i = 0; i < 1000; i++)
            {
                postss.Add(new() { Id = i + 1 });
            }

            _connection.RemoveRange(postss);

            try
            {
                _connection.Single(Lt.Query<Post>().Where(_ => _.Id == Lt.Arg<int>("Id")), new { Id = postss[0].Id });
                Assert.Fail();
            }
            catch { }
        }
    }

    static readonly Query<Tag> _getTag = Lt.Query<Tag>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void AddUpdateRemove_UsingUnitOfWork()
    {
        int id;
        Tag tag;
        using (var transaction = _connection.BeginTransaction())
        {
            using (var unitOfWork = _connection.CreateUnitOfWork())
            {
                tag = new Tag("tag");
                Assert.Equal(0, tag.Id);

                unitOfWork.Add(tag);

                Assert.Equal(0, tag.Id);

                unitOfWork.Commit();

                id = tag.Id;
                Assert.NotEqual(0, id);
            }

            using (var unitOfWork = _connection.CreateUnitOfWork())
            {
                tag = unitOfWork.Single(_getTag, new { Id = id });
                Assert.NotNull(tag);
                tag.Name = "NewName";

                unitOfWork.Update(tag);

                unitOfWork.Commit();
            }

            tag = _connection.Single(_getTag, new { Id = id });
            Assert.NotNull(tag);
            Assert.Equal("NewName", tag.Name);

            using (var unitOfWork = _connection.CreateUnitOfWork())
            {
                tag = unitOfWork.Single(_getTag, new { Id = id });
                Assert.NotNull(tag);

                unitOfWork.Remove(tag);

                unitOfWork.Commit();
            }
        }

        try
        {
            tag = _connection.Single(_getTag, new { Id = id });
            Assert.Fail();
        }
        catch { }
    }
}
