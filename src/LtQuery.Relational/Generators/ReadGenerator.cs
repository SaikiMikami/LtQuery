#if !RELEASE
//#define SaveDynamicAssmembly
#endif

using LtQuery.Metadata;
using LtQuery.Relational.Nodes;
using System.Data.Common;
using System.Reflection.Emit;
#if SaveDynamicAssmembly
using Lokad.ILPack;
using System.Reflection;
#endif

namespace LtQuery.Relational.Generators;

class ReadGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    public ReadGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "__Read";
    static int _no = 0;

    public ExecuteSelect<TEntity> CreateReadSelectFunc<TEntity>(Query<TEntity> query) where TEntity : class
    {
#if SaveDynamicAssmembly
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_no++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbDataReader) });
#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbDataReader) }, GetType().Module, true);
#endif

        var il = method.GetILGenerator();

        var root = Root.Create(_metaService, query);

        var queryGenerator = new QueryGenerator(null, root.RootQuery);

        // loc_0
        var entities = il.DeclareLocal(typeof(List<TEntity>));

        // var entities = new List<TEntity>();
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Type.EmptyTypes)!);
        il.EmitStloc(entities);

        queryGenerator.EmitSelect(il);

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if SaveDynamicAssmembly
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);
        return (ExecuteSelect<TEntity>)Delegate.CreateDelegate(typeof(ExecuteSelect<TEntity>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<ExecuteSelect<TEntity>>();
#endif
    }

#if SaveDynamicAssmembly
    const string _assemblyName = "DynamicAssmembly";
    const string _className = "__Dynamic_A";
    static void save(string path, Type type)
    {
        var assembly = Assembly.GetAssembly(type);
        var generator = new AssemblyGenerator();
        generator.GenerateAssembly(assembly, path);
    }
#endif
}
