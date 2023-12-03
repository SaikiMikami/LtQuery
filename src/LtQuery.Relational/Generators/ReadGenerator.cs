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
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbCommand) });
#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbCommand) }, GetType().Module, true);
#endif

        var il = method.GetILGenerator();

        var root = Root.Create(_metaService, query);

        var queryGenerator = new QueryGenerator(null, root.RootQuery);

        // loc_0
        var entities = il.DeclareLocal(typeof(List<TEntity>));
        // loc_1
        var reader = il.DeclareLocal(typeof(DbDataReader));

        // var entities = new List<TEntity>();
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Type.EmptyTypes)!);
        il.EmitStloc(entities);

        // using(var reader = command.ExecuteReader())
        il.Emit(OpCodes.Ldarg_0);
        il.EmitCall(DbCommand_ExecuteReader);
        il.EmitStloc(reader);
        // try
        il.BeginExceptionBlock();
        {
            queryGenerator.EmitSelect(il, reader);
        }
        // finally
        {
            il.BeginFinallyBlock();
            var finallyEnd = il.DefineLabel();
            il.EmitLdloc(reader);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(reader);
            il.EmitCall(IDisposable_Dispose);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();
        }

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
