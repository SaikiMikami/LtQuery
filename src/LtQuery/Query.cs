using LtQuery.Elements;

namespace LtQuery;

public class Query<TEntity> : AbstractImmutable where TEntity : class
{
    public IBoolValue? Condition { get; }
    public ImmutableArray<Include> Includes { get; }
    public IValue? SkipCount { get; }
    public IValue? TakeCount { get; }
    public ImmutableArray<OrderBy> OrderBys { get; }

    public Query(IBoolValue? condition = default, ImmutableArray<Include>? includes = default, ImmutableArray<OrderBy>? orderBys = default, IValue? skipCount = default, IValue? takeCount = default)
    {
        Condition = condition;
        Includes = includes ?? new(Array.Empty<Include>());
        OrderBys = orderBys ?? new(Array.Empty<OrderBy>());
        SkipCount = skipCount;
        TakeCount = takeCount;
    }

    protected override int CreateHashCode()
    {
        var code = 0;
        AddHashCode(ref code, Condition);
        AddHashCode(ref code, Includes);
        AddHashCode(ref code, SkipCount);
        AddHashCode(ref code, TakeCount);
        return code;
    }
}
