//#define DynamicAssmemblySave

using LtQuery.Metadata;
using System.Data.Common;
using System.Reflection.Emit;
#if DynamicAssmemblySave
using Lokad.ILPack;
using System.Reflection;
#endif

namespace LtQuery.Relational.Generators;

class ReadGenerator<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    public ReadGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "Read";
    static int _no = 0;

#if DynamicAssmemblySave
    const string _assemblyName = "DynamicAssmembly";
    const string _className = "__Dynamic_A";
    static void save(string path, Type type)
    {
        var assembly = Assembly.GetAssembly(type);
        var generator = new AssemblyGenerator();
        generator.GenerateAssembly(assembly, path);
    }
#endif

    public Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>> CreateReadSelectFunc(Query<TEntity> query)
    {
#if DynamicAssmemblySave
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_no++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(IReadOnlyList<TEntity>), new Type[] { typeof(IReadOnlyList<DbCommand>) });

#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", typeof(IReadOnlyList<TEntity>), new Type[] { typeof(IReadOnlyList<DbCommand>) }, GetType().Module, true);
#endif


        var il = method.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();
        var node = new QueryNode(meta, query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);

        var entities = il.DeclareLocal(typeof(List<TEntity>));

        // var entities = new List<TEntity>();
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
        il.EmitStloc(entities);

        var index = 0;
        var tree = new QueryTree(node, query.Condition, query.SkipCount, query.TakeCount, ref index);
        tree.EmitSelect(il);

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if DynamicAssmemblySave
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<IReadOnlyList<DbCommand>, IReadOnlyList<TEntity>>>();
#endif
    }

    public Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>> CreateReadSelectFunc<TParameter>(Query<TEntity> query)
    {
#if DynamicAssmemblySave
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_no++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(IReadOnlyList<TEntity>), new Type[] { typeof(IReadOnlyList<DbCommand>), typeof(TParameter) });
#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", typeof(IReadOnlyList<TEntity>), new Type[] { typeof(IReadOnlyList<DbCommand>), typeof(TParameter) }, GetType().Module, true);
#endif
        var il = method.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();
        var node = new QueryNode(meta, query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);

        var entities = il.DeclareLocal(typeof(List<TEntity>));

        // var entities = new List<TEntity>();
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
        il.EmitStloc(entities);

        var index = 0;
        var tree = new QueryTree(node, query.Condition, query.SkipCount, query.TakeCount, ref index);
        tree.EmitSelect<TParameter>(il);

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if DynamicAssmemblySave
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<IReadOnlyList<DbCommand>, TParameter, IReadOnlyList<TEntity>>>();
#endif
    }
}
