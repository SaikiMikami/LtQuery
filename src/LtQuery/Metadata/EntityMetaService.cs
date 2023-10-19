using System.Reflection;

namespace LtQuery.Metadata;

public class EntityMetaService
{
    public EntityMetaService(IModelConfiguration modelConfiguration)
    {
        var modelBuilder = new ModelBuilder();
        modelConfiguration.Configure(modelBuilder);
        var metas = modelBuilder.Build();
        foreach (var meta in metas)
            addCache(meta);
    }

    static void addCache(EntityMeta meta)
    {
        var method = typeof(Cache<>).MakeGenericType(meta.Type).GetMethod("SetCache", BindingFlags.Public | BindingFlags.Static)!;
        method.Invoke(null, new object?[] { meta });
    }


    static class Cache<TEntity> where TEntity : class
    {
        public static EntityMeta? EntityMeta { get; private set; }

        public static void SetCache(EntityMeta meta) => EntityMeta = meta;
    }

    public EntityMeta GetEntityMeta<TEntity>() where TEntity : class => Cache<TEntity>.EntityMeta ?? throw new InvalidProgramException();

    public static EntityMeta GetEntityMeta(Type type)
    {
        var type0 = typeof(EntityMetaService);
        var method = type0.GetMethod(nameof(GetEntityMeta), 1, Array.Empty<Type>())!;
        method = method.MakeGenericMethod(type);
        return (EntityMeta)method.Invoke(null, null)!;
    }
}
