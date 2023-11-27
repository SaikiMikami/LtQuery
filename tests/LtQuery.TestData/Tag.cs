namespace LtQuery.TestData;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<BlogTag> BlogTags { get; set; } = new();

    public Tag(string name)
    {
        Name = name;
    }

    public Tag(int id, string name)
    {
        Id = id;
        Name = name;
    }
#pragma warning disable CS8618
    public Tag() { }
#pragma warning restore CS8618
}
