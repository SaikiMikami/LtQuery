namespace LtQuery.TestData;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }

    public List<Blog> Blogs { get; set; } = new();

    public Category(string name)
    {
        Name = name;
    }
    public Category(int id, string name)
    {
        Id = id;
        Name = name;
    }
#pragma warning disable CS8618
    public Category() { }
#pragma warning restore CS8618
}
