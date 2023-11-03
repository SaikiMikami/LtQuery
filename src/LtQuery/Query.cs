using LtQuery.Elements;

namespace LtQuery;

/// <summary>
/// Query object.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class Query<TEntity> : AbstractImmutable, IEquatable<Query<TEntity>> where TEntity : class
{
    /// <summary>
    /// Where clause condition
    /// </summary>
    public IBoolValue? Condition { get; }

    /// <summary>
    /// Entities to include in the query results
    /// </summary>
    public ImmutableArray<Include> Includes { get; }

    /// <summary>
    /// Skip count
    /// </summary>
    public IValue? SkipCount { get; }

    /// <summary>
    /// Take count
    /// </summary>
    public IValue? TakeCount { get; }

    /// <summary>
    /// Sorts the entities
    /// </summary>
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

    public override bool Equals(object? obj) => Equals(obj as Query<TEntity>);
    public bool Equals(Query<TEntity>? other)
    {
        if(other == null)
            return false;

        if (!Equals(Condition, other.Condition))
            return false;
        if (!Includes.Equals(other.Includes))
            return false;
        if (!Equals(SkipCount, other.SkipCount))
            return false;
        if (!Equals(TakeCount, other.TakeCount))
            return false;
        if (!Equals(OrderBys, other.OrderBys))
            return false;
        return true;
    }
}
