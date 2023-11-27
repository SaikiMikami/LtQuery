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
            _selectSingle(context);
            _selectSimple(context).ToArray();
            _selectIncludeChilren(context, 20).ToArray();
            _selectComplex(context, "PLCJKJKRUK", 10, 20).ToArray();
            context.Add(new Tag("a"));
            context.SaveChanges();
        }
    }
    static readonly Func<TestContext, Blog> _selectSingle
        = EF.CompileQuery((TestContext context) => context.Set<Blog>().Single(_ => _.Id == 1));
    static readonly Func<TestContext, IEnumerable<Blog>> _selectSimple
        = EF.CompileQuery((TestContext context) => context.Set<Blog>().OrderBy(_ => _.Id).Take(20).AsNoTracking());
    static readonly Func<TestContext, int, IEnumerable<Blog>> _selectIncludeChilren
        = EF.CompileQuery((TestContext context, int id) => context.Set<Blog>().Include(_ => _.Posts).Where(_ => _.Id < id).AsNoTracking());
    static readonly Func<TestContext, string, int, int, IEnumerable<Blog>> _selectComplex
        = EF.CompileQuery((TestContext context, string userName, int skipCount, int takeCount) => context.Set<Blog>().Include(_ => _.User).Include(_ => _.Posts).ThenInclude(_ => _.User).Where(_ => _.Posts.Any(_ => _.User!.Name == userName)).OrderBy(_ => _.Id).Skip(skipCount).Take(takeCount).AsNoTracking());

    public void Cleanup()
    {
    }


    public int SelectSingle()
    {
        Blog entity;
        using (var context = new TestContext())
        {
            entity = _selectSingle(context);
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
            entities = _selectSimple(context).ToArray();
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
            entities = _selectIncludeChilren(context, 20).ToArray();
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
            entities = _selectComplex(context, "PLCJKJKRUK", 10, 20).ToArray();
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
