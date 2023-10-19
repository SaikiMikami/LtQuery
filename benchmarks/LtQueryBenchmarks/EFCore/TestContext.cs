using LtQuery.TestData;
using Microsoft.EntityFrameworkCore;

namespace LtQueryBenchmarks.EFCore
{
    class TestContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Constants.ConnectionString, _ => _.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            //optionsBuilder.LogTo(Console.WriteLine);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(_ => _.Id);
            });
            modelBuilder.Entity<Category>(b =>
            {
                b.HasKey(_ => _.Id);
            });
            modelBuilder.Entity<Blog>(b =>
            {
                b.HasKey(_ => _.Id);
                b.HasOne(_ => _.Category).WithMany(_ => _.Blogs).HasForeignKey(_ => _.CategoryId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(_ => _.User).WithMany(_ => _.Blogs).HasForeignKey(_ => _.UserId).OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Post>(b =>
            {
                b.HasKey(_ => _.Id);
                b.HasOne(_ => _.Blog).WithMany(_ => _.Posts).HasForeignKey(_ => _.BlogId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(_ => _.User).WithMany(_ => _.Posts).HasForeignKey(_ => _.UserId).OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<Tag>(b =>
            {
                b.HasKey(_ => _.Id);
            });
            modelBuilder.Entity<BlogTag>(b =>
            {
                b.HasKey(_ => new { _.BlogId, _.TagId });
                b.HasOne(_ => _.Blog).WithMany(_ => _.BlogTags).HasForeignKey(_ => _.BlogId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(_ => _.Tag).WithMany(_ => _.BlogTags).HasForeignKey(_ => _.TagId).OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}
