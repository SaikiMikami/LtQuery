using LtQuery.Metadata;

namespace LtQuery.TestData;

public class ModelConfiguration : IModelConfiguration
{
    public void Configure(IModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasProperty(_ => _.Password);
        });
        modelBuilder.Entity<User>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasProperty(_ => _.Name);
            b.HasProperty(_ => _.Email);
            b.HasReference(_ => _.AccountId, _ => _.Account, _ => _.User);
        });
        modelBuilder.Entity<Category>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasProperty(_ => _.Name);
        });
        modelBuilder.Entity<Blog>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasProperty(_ => _.Title);
            b.HasReference(_ => _.CategoryId, _ => _.Category, _ => _.Blogs);
            b.HasReference(_ => _.UserId, _ => _.User, _ => _.Blogs);
            b.HasProperty(_ => _.DateTime);
            b.HasProperty(_ => _.Content);
        });
        modelBuilder.Entity<Post>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasReference(_ => _.BlogId, _ => _.Blog, _ => _.Posts);
            b.HasReference(_ => _.UserId, _ => _.User, _ => _.Posts);
            b.HasProperty(_ => _.DateTime);
            b.HasProperty(_ => _.Content);
        });
        modelBuilder.Entity<Tag>(b =>
        {
            b.HasProperty(_ => _.Id, true);
            b.HasProperty(_ => _.Name);
        });
        modelBuilder.Entity<BlogTag>(b =>
        {
            b.HasReference(_ => _.BlogId, _ => _.Blog, _ => _.BlogTags);
            b.HasReference(_ => _.TagId, _ => _.Tag, _ => _.BlogTags);
        });
    }
}
