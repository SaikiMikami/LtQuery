#if !RELEASE
//#define SaveDynamicAssmembly
#endif

using LtQuery.Metadata;
using LtQuery.Relational.Nodes;
using LtQuery.Relational.Nodes.Values;
using System.Data.Common;
using System.Reflection.Emit;
#if SaveDynamicAssmembly
using Lokad.ILPack;
using System.Reflection;
#endif

namespace LtQuery.Relational.Generators;

class ReadGenerator<TEntity> : AbstractGenerator where TEntity : class
{
    readonly EntityMetaService _metaService;
    public ReadGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "Read";
    static int _no = 0;

    public Func<DbCommand, IReadOnlyList<TEntity>> CreateReadSelectFunc(Query<TEntity> query)
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
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
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

        return (Func<DbCommand, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<DbCommand, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<DbCommand, IReadOnlyList<TEntity>>>();
#endif
    }

    public Func<DbCommand, TParameter, IReadOnlyList<TEntity>> CreateReadSelectFunc<TParameter>(Query<TEntity> query)
    {
#if SaveDynamicAssmembly
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

        var root = Root.Create(_metaService, query);

        var queryGenerator = new QueryGenerator(null, root.RootQuery);

        // loc_0
        var entities = il.DeclareLocal(typeof(List<TEntity>));
        // loc_1
        var reader = il.DeclareLocal(typeof(DbDataReader));

        // var entities = new List<TEntity>();
        il.Emit(OpCodes.Newobj, typeof(List<TEntity>).GetConstructor(Array.Empty<Type>())!);
        il.EmitStloc(entities);

        emitParameters<TParameter>(il, root.AllParameters);

        // using(var reader = command.ExecuteReader())
        il.Emit(OpCodes.Ldarg_0);
        il.EmitCall(DbCommand_ExecuteReader);
        il.EmitStloc(reader);
        il.BeginExceptionBlock();
        {
            queryGenerator.EmitSelect(il, reader);
        }
        // finally
        {
            // reader?.Dispose()
            var ifEnd = il.DefineLabel();
            il.BeginFinallyBlock();
            il.EmitLdloc(reader);
            il.Emit(OpCodes.Brfalse_S, ifEnd);
            il.EmitLdloc(reader);
            il.EmitCall(IDisposable_Dispose);
            il.MarkLabel(ifEnd);
            il.EndExceptionBlock();
        }

        il.EmitLdloc(entities);
        il.Emit(OpCodes.Ret);

#if SaveDynamicAssmembly
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Func<DbCommand, TParameter, IReadOnlyList<TEntity>>)Delegate.CreateDelegate(typeof(Func<DbCommand, TParameter, IReadOnlyList<TEntity>>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<Func<DbCommand, TParameter, IReadOnlyList<TEntity>>>();
#endif
    }


    const string _updateMethodName = "Update";
    static int _updateNo = 0;


    public Action<DbCommand, IEnumerable<TEntity>> CreateExecuteUpdateFunc(DbMethod dbMethod)
    {
#if SaveDynamicAssmembly
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_updateNo++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_updateMethodName, MethodAttributes.Public | MethodAttributes.Static, null, new Type[] { typeof(DbCommand), typeof(IEnumerable<TEntity>) });

#else
        var method = new DynamicMethod($"{_updateMethodName}_{_updateNo++}", null, new Type[] { typeof(DbCommand), typeof(IEnumerable<TEntity>) }, GetType().Module, true);
#endif
        var il = method.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();

        var enumerator = il.DeclareLocal(typeof(IEnumerator<TEntity>));
        var reader = il.DeclareLocal(typeof(DbDataReader));

        // foreach(var entity in entities)
        var entity = il.DeclareLocal(typeof(TEntity));
        var foreachEnd = il.DefineLabel();
        var tryStart = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_1);
        il.EmitCall(Cast<TEntity>.IEnumerable_GetEnumerator);
        il.EmitStloc(enumerator);
        // try
        il.BeginExceptionBlock();
        il.Emit(OpCodes.Br, foreachEnd);
        {
            il.MarkLabel(tryStart);

            // entity = enumerator.Current
            il.EmitLdloc(enumerator);
            il.EmitCall(Cast<TEntity>.IEnumerator_get_Current);
            il.EmitStloc(entity);

            // command.Parameter[XXX].Value = Entity.XXX
            emitAddParameters(il, entity, dbMethod);

            // using(var reader = command.ExecuteReader())
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(DbCommand_ExecuteReader);
            il.EmitStloc(reader);
            il.BeginExceptionBlock();
            {
                // reader.Read()
                il.EmitLdloc(reader);
                il.EmitCall(DbDataReader_Read);
                il.Emit(OpCodes.Pop);
                if (dbMethod == DbMethod.Add)
                {
                    // entity.Id = reader.GetXXX(0)
                    il.EmitLdloc(entity);
                    EmitCallReader_Read(il, reader, meta.Key.Type, 0);
                    il.EmitCall(meta.Key.Info.GetSetMethod()!);
                }
            }
            // finally
            {
                // reader?.Dispose()
                var ifEnd = il.DefineLabel();
                il.BeginFinallyBlock();
                il.EmitLdloc(reader);
                il.Emit(OpCodes.Brfalse_S, ifEnd);
                il.EmitLdloc(reader);
                il.EmitCall(IDisposable_Dispose);
                il.MarkLabel(ifEnd);
                il.EndExceptionBlock();
            }
        }
        // foreach block end
        il.MarkLabel(foreachEnd);
        il.EmitLdloc(enumerator);
        il.EmitCall(IEnumerator_MoveNext);
        il.Emit(OpCodes.Brtrue, tryStart);

        // finally
        {
            var finallyEnd = il.DefineLabel();
            il.BeginFinallyBlock();
            il.EmitLdloc(enumerator);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(enumerator);
            il.EmitCall(IDisposable_Dispose);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();
        }

        il.Emit(OpCodes.Ret);

#if SaveDynamicAssmembly
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (Action<DbCommand, IEnumerable<TEntity>>)Delegate.CreateDelegate(typeof(Action<DbCommand, IEnumerable<TEntity>>), type2.GetMethod(_updateMethodName)!);
#else
        return method.CreateDelegate<Action<DbCommand, IEnumerable<TEntity>>>();
#endif
    }

    void emitParameters<TParameter>(ILGenerator il, IReadOnlyList<ParameterValueData> parameters)
    {
        for (var i = 0; i < parameters.Count; i++)
        {
            // command.Parameter[XXX].Value = parameters.XXX
            var parameter = parameters[i];
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(DbCommand_get_Parameters);
            il.EmitLdc_I4(i);
            il.EmitCall(DbParameterCollection_get_Item);
            il.Emit(OpCodes.Ldarg_1);
            var method = typeof(TParameter).GetProperty(parameter.Name)?.GetGetMethod() ?? throw new InvalidOperationException($"Argument [{parameter.Name}] is not included in the query");
            il.EmitCall(method);
            var parameterType = parameters[i].Type;
            if (parameterType.IsValueType)
                il.Emit(OpCodes.Box, parameterType);

            // ?? DbNull.Value
            var label = il.DefineLabel();
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Brtrue_S, label);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ldsfld, DBNullType_Value);
            il.MarkLabel(label);

            il.EmitCall(DbParameter_set_Value);
        }
    }

    void emitAddParameters(ILGenerator il, LocalBuilder entity, DbMethod dbMethod)
    {
        var meta = _metaService.GetEntityMeta<TEntity>();
        var index = 0;
        for (var i = 0; i < meta.Properties.Count; i++)
        {
            var property = meta.Properties[i];
            switch (dbMethod)
            {
                case DbMethod.Add:
                    if (property.IsKey)
                        continue;   // FIXME: not support add auto increment
                    break;
                case DbMethod.Remove:
                    if (!property.IsKey)
                        continue;
                    break;
            }

            // command.Parameter[XXX].Value = Entity.XXX
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(DbCommand_get_Parameters);
            il.EmitLdc_I4(index);
            il.EmitCall(DbParameterCollection_get_Item);

            il.EmitLdloc(entity);
            var method = typeof(TEntity).GetProperty(property.Name)?.GetGetMethod() ?? throw new InvalidOperationException($"Argument [{property.Name}] is not included in the query");
            il.EmitCall(method);

            var parameterType = property.Type;
            if (parameterType.IsNullable())
            {
                var nullable = il.DeclareLocal(parameterType);
                var label1 = il.DefineLabel();
                var label2 = il.DefineLabel();

                il.EmitStloc(nullable);
                il.EmitLdloca_S(nullable);
                il.EmitCall(parameterType.GetProperty("HasValue")!.GetGetMethod()!);
                il.Emit(OpCodes.Brtrue_S, label1);

                il.Emit(OpCodes.Ldsfld, DBNullType_Value);
                il.Emit(OpCodes.Br_S, label2);

                il.MarkLabel(label1);
                il.EmitLdloca_S(nullable);
                il.EmitCall(parameterType.GetProperty("Value")!.GetGetMethod()!);
                il.Emit(OpCodes.Box, parameterType.GenericTypeArguments[0]);

                il.MarkLabel(label2);
            }
            else
            {
                if (parameterType.IsValueType)
                    il.Emit(OpCodes.Box, parameterType);

                // ?? DbNull.Value
                if (parameterType.IsClass)
                {
                    var label = il.DefineLabel();
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Brtrue_S, label);
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldsfld, DBNullType_Value);
                    il.MarkLabel(label);
                }
            }

            il.EmitCall(DbParameter_set_Value);
            index++;
        }
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
