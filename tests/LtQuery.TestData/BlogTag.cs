namespace LtQuery.TestData;

public class BlogTag
{
    public int BlogId { get; set; }
    public int TagId { get; set; }

    public Blog Blog { get; set; } = default!;
    public Tag Tag { get; set; } = default!;

    public BlogTag(int blogId, int tagId)
    {
        BlogId = blogId;
        TagId = tagId;
    }
#pragma warning disable CS8618
    public BlogTag() { }
#pragma warning restore CS8618
}
