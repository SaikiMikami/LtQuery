using Microsoft.EntityFrameworkCore;

namespace LtQueryBenchmarks;

internal class RandomEx : Random
{
    public RandomEx(int seed) : base(seed) { }

    public char NextChar() => (char)(Next() % (0x5a - 0x41) + 0x41);
    public string NextString(int count = 10)
    {
        var str = string.Empty;
        for (var i = 0; i < count; i++)
            str += NextChar();
        return str;
    }
    public DateTime NextDateTime() => new DateTime(Next() % 20 + 2000, Next() % 12 + 1, Next() % 20 + 1);

    public int NextEntityId<TEntity>(DbContext context) where TEntity : class
    {
        var count = context.Set<TEntity>().Count();
        return Next() % count + 1;
    }
    public TEntity NextEntity<TEntity>(DbContext context) where TEntity : class
    {
        var id = NextEntityId<TEntity>(context);
        return context.Set<TEntity>().Find(id)!;
    }
}
