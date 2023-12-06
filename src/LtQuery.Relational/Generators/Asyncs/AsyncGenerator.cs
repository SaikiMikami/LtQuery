using LtQuery.Metadata;
using System.Reflection;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators.Asyncs;

class AsyncGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    public AsyncGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _assemblyName = "DynamicAssmembly";
    const string _moduleName = "DynamicModule";

    static ModuleBuilder? __moduleBuilder;
    static ModuleBuilder _moduleBuilder
    {
        get
        {
            if (__moduleBuilder == null)
            {
                var assmName = new AssemblyName(_assemblyName);
                var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
                __moduleBuilder = assm.DefineDynamicModule(_moduleName);
            }
            return __moduleBuilder;
        }
    }

    public ExecuteSelectAsync<TEntity> CreateReadSelectAsyncFunc<TEntity>(Query<TEntity> query) where TEntity : class
    {
        var selector = AsyncSelectGenerator.Create<TEntity>(_metaService, _moduleBuilder);
        var type = selector.Build(query);

        return type.GetMethod("Execute")!.CreateDelegate<ExecuteSelectAsync<TEntity>>();
    }
}
