namespace LtQuery.TestData;

public class Post
{
    public int Id { get; set; }
    public int BlogId { get; set; }
    public int? UserId { get; set; }
    public DateTime DateTime { get; set; }
    public string Content { get; set; }

    public Blog Blog { get; set; } = default!;
    public User? User { get; set; }

    public Post(int blogId, int? userId, DateTime dateTime, string content)
    {
        BlogId = blogId;
        UserId = userId;
        DateTime = dateTime;
        Content = content;
    }
    public Post(int id, int blogId, int? userId, DateTime dateTime, string content)
    {
        Id = id;
        BlogId = blogId;
        UserId = userId;
        DateTime = dateTime;
        Content = content;
    }
#pragma warning disable CS8618
    public Post() { }
#pragma warning restore CS8618
}
