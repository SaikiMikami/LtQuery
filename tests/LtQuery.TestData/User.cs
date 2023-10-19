namespace LtQuery.TestData
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public List<Blog> Blogs { get; set; } = new();
        public List<Post> Posts { get; set; } = new();

        public User(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }
#pragma warning disable CS8618
        public User() { }
#pragma warning restore CS8618
    }
}
