using LtQuery.TestData;
using Microsoft.EntityFrameworkCore;

namespace LtQueryBenchmarks.EFCore;

class EFCoreBenchmark : AbstractBenchmark
{
    public void Setup()
    {
        using (var context = new TestContext())
        {
            _selectSingle(context);
            _selectSimple(context).ToArray();
            _selectAllIncludeUniqueMany(context).ToArray();
            _selectIncludeChilren(context, 20).ToArray();
        }
    }
    static readonly Func<TestContext, Blog> _selectSingle
        = EF.CompileQuery((TestContext context) => context.Set<Blog>().Single(_ => _.Id == 1));
    static readonly Func<TestContext, IEnumerable<Blog>> _selectSimple
        = EF.CompileQuery((TestContext context) => context.Set<Blog>().OrderBy(_ => _.Id).Take(20).AsNoTracking());
    static readonly Func<TestContext, IEnumerable<Blog>> _selectAllIncludeUniqueMany
        = EF.CompileQuery((TestContext context) => context.Set<Blog>().Include(_ => _.Posts).AsNoTracking());
    static readonly Func<TestContext, int, IEnumerable<Blog>> _selectIncludeChilren
        = EF.CompileQuery((TestContext context, int id) => context.Set<Blog>().Include(_ => _.Posts).Where(_ => _.Id < id).AsNoTracking());

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
        var accum = 0;
        Blog[] entities;

        using (var context = new TestContext())
        {
            entities = _selectSimple(context).ToArray();
        }

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    public int SelectAllIncludeUniqueMany()
    {
        var accum = 0;
        Blog[] entities;

        using (var context = new TestContext())
        {
            entities = _selectAllIncludeUniqueMany(context).ToArray();
        }

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
        }
        return accum;
    }

    public int SelectIncludeChilren()
    {
        var accum = 0;
        Blog[] entities;

        using (var context = new TestContext())
        {
            entities = _selectIncludeChilren(context, 20).ToArray();
        }

        foreach (var entity in entities)
        {
            AddHashCode(ref accum, entity.Id);
            foreach (var post in entity.Posts)
                AddHashCode(ref accum, post.Id);
        }
        return accum;
    }
}
