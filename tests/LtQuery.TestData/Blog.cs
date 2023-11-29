namespace LtQuery.TestData;

public class Blog
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    public DateTime DateTime { get; set; }
    public string Content { get; set; }

    public Category Category { get; set; } = default!;
    public User User { get; set; } = default!;
    public List<BlogTag> BlogTags { get; set; } = default!;
    public List<Post> Posts { get; } = new();

    public Blog(string title, Category category, User user, DateTime dateTime, string content)
    {
        Title = title;
        Category = category;
        User = user;
        DateTime = dateTime;
        Content = content;

        CategoryId = category.Id;
        UserId = user.Id;
    }
    public Blog(int id, string title, int categoryId, int userId, DateTime dateTime, string content)
    {
        Id = id;
        Title = title;
        CategoryId = categoryId;
        UserId = userId;
        DateTime = dateTime;
        Content = content;
    }
#pragma warning disable CS8618
    public Blog() { }
#pragma warning restore CS8618
}
