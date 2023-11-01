using LtQuery.TestData;
using LtQueryBenchmarks.EFCore;

namespace LtQueryBenchmarks
{
    class InitDataFactory
    {
        public void Create()
        {
            var rand = new RandomEx(0);

            using (var context = new TestContext())
            {
                for (var i = 0; i < 100; i++)
                {
                    var user = new User()
                    {
                        Name = rand.NextString(),
                        Email = rand.NextString(),
                    };
                    context.Add(user);
                }
                context.SaveChanges();
            }
            using (var context = new TestContext())
            {
                for (var i = 0; i < 10; i++)
                {
                    var category = new Category()
                    {
                        Name = rand.NextString(),
                    };
                    context.Add(category);
                }
                context.SaveChanges();
            }
            for (var i = 0; i < 10000; i++)
            {
                using (var context = new TestContext())
                {
                    var blog = new Blog()
                    {
                        Title = rand.NextString(),
                        CategoryId = rand.Next() % 10 + 1,
                        UserId = rand.Next() % 10 + 1,
                        DateTime = rand.NextDateTime(),
                        Content = rand.NextString(500),
                    };
                    context.Add(blog);

                    for (var j = 0; j < 100; j++)
                    {
                        var post = new Post()
                        {
                            Blog = blog,
                            UserId = j % 2 == 0 ? null : rand.Next() % 10 + 1,
                            DateTime = rand.NextDateTime(),
                            Content = rand.NextString(100),
                        };
                        context.Add(post);
                    }
                    context.SaveChanges();
                }
            }

        }
    }
}
