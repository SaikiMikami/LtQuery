namespace LtQuery.Metadata;

class ModelBuilder : IModelBuilder
{
    public Dictionary<Type, IEntityTypeBuilder> EntityTypeBuilders { get; } = new();
    public IModelBuilder Entity<TEntity>(Action<IEntityTypeBuilder<TEntity>> buildAction) where TEntity : class
    {
        var entityTypeBuilder = new EntityTypeBuilder<TEntity>(this);
        buildAction(entityTypeBuilder);
        EntityTypeBuilders.Add(entityTypeBuilder.Meta.Type, entityTypeBuilder);
        return this;
    }

    bool _isBuilded = false;
    public IReadOnlyList<EntityMeta> Build()
    {
        if (!_isBuilded)
        {
            foreach (var entityTypeBuilder in EntityTypeBuilders.Values)
                entityTypeBuilder.Finish();
            _isBuilded = true;
        }
        return EntityTypeBuilders.Values.Select(_ => _.Meta).ToArray();
    }
}
