namespace LtQuery.TestData;

public class Account
{
    public string Id { get; set; }
    public string Password { get; set; }

    public User? User { get; set; }

    public Account(string id, string password)
    {
        Id = id;
        Password = password;
    }
#pragma warning disable CS8618
    public Account() { }
#pragma warning restore CS8618
}
