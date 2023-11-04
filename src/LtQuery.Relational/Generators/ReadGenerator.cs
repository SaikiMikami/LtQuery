//#define DynamicAssmemblySave

using LtQuery.Elements;
using LtQuery.Elements.Values;
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


    public Func<DbCommand, IReadOnlyList<TEntity>> CreateReadSelectFunc(Query<TEntity> query)
    {
#if DynamicAssmemblySave
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

        var meta = _metaService.GetEntityMeta<TEntity>();
        var node = new QueryNode(meta, query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);

        var index = 0;
        var tree = new QueryTree(node, query.Condition, query.SkipCount, query.TakeCount, ref index);

        // loc_0
        var entities = il.DeclareLocal(typeof(List<TEntity>));
        // loc_1
        var reader = il.DeclareLocal(typeof(DbDataReader));

        // using(var reader = command.ExecuteReader())
        il.Emit(OpCodes.Ldarg_0);
        il.EmitCall(typeof(DbCommand).GetMethod("ExecuteReader", Array.Empty<Type>())!);
        il.EmitStloc(reader);
        // try
        il.BeginExceptionBlock();
        {
            // var entities = new List<TEntity>();
            il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
            il.EmitStloc(entities);

            tree.EmitSelect(il);
        }
        // finally
        {
            il.BeginFinallyBlock();
            var finallyEnd = il.DefineLabel();
            il.EmitLdloc(reader);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(reader);
            il.EmitCall(typeof(IDisposable).GetMethod("Dispose")!);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();

            //il.Emit(OpCodes.Endfinally);
        }

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if DynamicAssmemblySave
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Func<DbCommand, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<DbCommand, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<DbCommand, IReadOnlyList<TEntity>>>();
#endif
    }

    public Func<DbCommand, TParameter, IReadOnlyList<TEntity>> CreateReadSelectFunc<TParameter>(Query<TEntity> query)
    {
#if DynamicAssmemblySave
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_no++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbCommand), typeof(TParameter) });
#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", typeof(IReadOnlyList<TEntity>), new Type[] { typeof(DbCommand), typeof(TParameter) }, GetType().Module, true);
#endif

        var il = method.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();
        var node = new QueryNode(meta, query.Condition, query.Includes, query.OrderBys, query.SkipCount, query.TakeCount);

        var index = 0;
        var tree = new QueryTree(node, query.Condition, query.SkipCount, query.TakeCount, ref index);


        // loc_0
        var entities = il.DeclareLocal(typeof(List<TEntity>));
        // loc_1
        var reader = il.DeclareLocal(typeof(DbDataReader));

        var parameters = getParameters(query);
        emitParameters<TParameter>(il, parameters);

        // using(var reader = command.ExecuteReader())
        var finallyEnd = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.EmitCall(typeof(DbCommand).GetMethod("ExecuteReader", Array.Empty<Type>())!);
        il.EmitStloc(reader);
        var tryStart = il.BeginExceptionBlock();
        {
            // var entities = new List<TEntity>();
            il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
            il.EmitStloc(entities);

            tree.EmitSelect(il);

            il.Emit(OpCodes.Leave_S, finallyEnd);
        }
        // finally
        {
            var ifEnd = il.DefineLabel();
            il.BeginFinallyBlock();
            il.EmitLdloc(reader);
            il.Emit(OpCodes.Brfalse_S, ifEnd);
            il.EmitLdloc(reader);
            il.EmitCall(typeof(IDisposable).GetMethod("Dispose")!);
            il.MarkLabel(ifEnd);
            il.EndExceptionBlock();

            //il.Emit(OpCodes.Endfinally);
        }
        il.MarkLabel(finallyEnd);

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if DynamicAssmemblySave
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Func<DbCommand, TParameter, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<DbCommand, TParameter, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<DbCommand, TParameter, IReadOnlyList<TEntity>>>();
#endif
    }

    static IReadOnlyList<ParameterValue> getParameters(Query<TEntity> query)
    {
        var parameters = new List<ParameterValue>();
        if (query.Condition != null)
        {
            buildParameterValues(parameters, query.Condition);
        }
        if (query.SkipCount != null)
        {
            buildParameterValues(parameters, query.SkipCount);
        }
        if (query.TakeCount != null)
        {
            buildParameterValues(parameters, query.TakeCount);
        }
        return parameters;
    }

    static void buildParameterValues(List<ParameterValue> list, IValue src)
    {
        switch (src)
        {
            case ParameterValue v0:
                list.Add(v0);
                break;
            case IBinaryOperator v1:
                buildParameterValues(list, v1.Lhs);
                buildParameterValues(list, v1.Rhs);
                break;
        }
    }

    private void emitParameters<TParameter>(ILGenerator il, IReadOnlyList<ParameterValue> parameters)
    {
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(typeof(DbCommand).GetProperty("Parameters")!.GetGetMethod()!);
            il.EmitLdc_I4(i);
            il.EmitCall(typeof(DbParameterCollection).GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
            il.Emit(OpCodes.Ldarg_1);
            var method = typeof(TParameter).GetProperty(parameter.Name)?.GetGetMethod() ?? throw new InvalidOperationException($"Argument [{parameter.Name}] is not included in the query");
            il.EmitCall(method);
            var parameterType = parameters[i].Type;
            if (parameterType.IsValueType)
                il.Emit(OpCodes.Box, parameterType);
            il.EmitCall(typeof(DbParameter).GetProperty("Value")!.GetSetMethod()!);
        }
    }
}
