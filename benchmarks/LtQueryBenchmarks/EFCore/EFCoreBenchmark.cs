using LtQuery.TestData;
using Microsoft.EntityFrameworkCore;

namespace LtQueryBenchmarks.EFCore;

class EFCoreBenchmark : AbstractBenchmark
{
    RandomEx _random = default!;
    public void Setup()
    {
        _random = new(0);

        using (var context = new TestContext())
        {
            SelectSingle();
            SelectSimple();
            SelectIncludeChilren();
            SelectComplex();
            context.Add(new Tag("a"));
            context.SaveChanges();
        }
    }

    public void Cleanup()
    {
    }


    public int SelectSingle()
    {
        Blog entity;
        using (var context = new TestContext())
        {
            entity = context.Set<Blog>().Single(x => x.Id == 1);
        }

        var accum = 0;
        AddHashCode(ref accum, entity.Id);
        return accum;
    }

    public int SelectSimple()
    {
        Blog[] entities;
        using (var context = new TestContext())
        {
            entities = context.Set<Blog>().OrderBy(_ => _.Id).Take(20).ToArray();
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    public int SelectIncludeChilren()
    {
        Blog[] entities;
        using (var context = new TestContext())
        {
            entities = context.Set<Blog>().Include(_ => _.Posts).Where(_ => _.Id < 20).ToArray();
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }

    public async Task<int> SelectIncludeChilrenAsync()
    {
        Blog[] entities;
        using (var context = new TestContext())
        {
            var id = 20;
            entities = await context.Set<Blog>().Include(_ => _.Posts).Where(_ => _.Id < id).AsNoTracking().ToArrayAsync();
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }

    public int SelectComplex()
    {
        Blog[] entities;
        using (var context = new TestContext())
        {
            entities = context.Set<Blog>().Include(_ => _.User).Include(_ => _.Posts).ThenInclude(_ => _.User).Where(_ => _.Posts.Any(_ => _.User!.Name == "PLCJKJKRUK")).OrderBy(_ => _.Id).Skip(10).Take(20).ToArray();
        }

        var accum = 0;
        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }

    public int AddRange()
    {
        using (var context = new TestContext())
        {
            var tags = new List<Tag>();
            for (var i = 0; i < 10; i++)
            {
                tags.Add(new(_random.NextString()));
            }
            context.AddRange(tags);
            context.SaveChanges();
        }
        return 0;
    }
}
