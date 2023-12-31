﻿using LtQuery.TestData;
using Microsoft.Extensions.DependencyInjection;

namespace LtQuery.Sqlite.Tests;

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
        .Where(_ => _.Blogs.Any(_ => _.DateTime < Lt.Arg<DateTime>("DateTime") && _.Posts.Any(_ => _.User!.Name == Lt.Arg<string>("UserName")))).OrderBy(_ => _.Id).Take("Take").ToImmutable();

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

    static readonly Query<Tag> _getTag = Lt.Query<Tag>().Where(_ => _.Id == Lt.Arg<int>("Id")).ToImmutable();

    [Fact]
    public void AddUpdateRemove_UsingUnitOfWork()
    {
        int id;
        Tag tag;
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

        try
        {
            tag = _connection.Single(_getTag, new { Id = id });
            Assert.Fail();
        }
        catch { }
    }

    [Fact]
    public void Add_WithChild()
    {
        var random = new RandomEx(0);
        using (var transaction = _connection.BeginTransaction())
        {
            var user = new User(random.NextString(), null, null);
            _connection.Add(user);
            var category = new Category(random.NextString());
            _connection.Add(category);
            var blog = new Blog(random.NextString(), category, user, random.NextDateTime(), random.NextString());
            _connection.Add(blog);

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
}