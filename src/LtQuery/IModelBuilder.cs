using LtQuery.Metadata;

namespace LtQuery;

public interface IModelBuilder
{
    IModelBuilder Entity<TEntity>(Action<IEntityTypeBuilder<TEntity>> buildAction) where TEntity : class;
}
