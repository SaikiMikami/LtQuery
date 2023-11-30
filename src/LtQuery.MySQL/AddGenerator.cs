#if !RELEASE
//#define SaveDynamicAssmembly
#endif
using LtQuery.Metadata;
using LtQuery.Relational;
using LtQuery.Relational.Generators;
using System.Data.Common;
using System.Reflection.Emit;
#if SaveDynamicAssmembly
using Lokad.ILPack;
using System.Reflection;
#endif

namespace LtQuery.MySql;

class AddGenerator<TEntity> : AbstractGenerator, IAddGenerator<TEntity> where TEntity : class
{
    readonly EntityMetaService _metaService;
    public AddGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "Add";
    static int _no = 0;

    public ExecuteUpdate<TEntity> CreateExecuteAddFunc()
    {
#if SaveDynamicAssmembly
        var assmName = new AssemblyName(_assemblyName);
        var assm = AssemblyBuilder.DefineDynamicAssembly(assmName, AssemblyBuilderAccess.Run);
        var module = assm.DefineDynamicModule("DynamicModule");
        var className = $"{_className}_{_no++}";
        var type = module.DefineType(className, TypeAttributes.Public, typeof(object));
        var method = type.DefineMethod(_methodName, MethodAttributes.Public | MethodAttributes.Static, null, new Type[] { typeof(DbCommand), typeof(Span<TEntity>) });
#else
        var method = new DynamicMethod($"{_methodName}_{_no++}", null, new Type[] { typeof(DbCommand), typeof(Span<TEntity>) }, GetType().Module, true);
#endif
        var il = method.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();

        var reader = il.DeclareLocal(typeof(DbDataReader));

        var index = il.DeclareLocal(typeof(int));
        il.EmitLdc_I4(0);
        il.EmitStloc(index);

        var i = il.DeclareLocal(typeof(int));
        var entity = il.DeclareLocal(typeof(TEntity));

        // for(var i = 0 ; i < entities.Length; i++)
        {
            var forStart = il.DefineLabel();
            var forEnd = il.DefineLabel();
            il.EmitLdc_I4(0);
            il.EmitStloc(i);
            il.Emit(OpCodes.Br, forEnd);
            {
                il.MarkLabel(forStart);

                // var entity = entities[i]
                il.EmitLdarga_S(1);
                il.EmitLdloc(i);
                il.EmitCall(Cast<TEntity>.Span_get_Item);
                il.Emit(OpCodes.Ldind_Ref);
                il.EmitStloc(entity);

                emitAddParameters(il, entity, index);

                // i++
                il.EmitLdloc(i);
                il.EmitLdc_I4(1);
                il.Emit(OpCodes.Add);
                il.EmitStloc(i);

                // foreach end
                il.MarkLabel(forEnd);
                il.EmitLdloc(i);
                il.EmitLdarga_S(1);
                il.EmitCall(Cast<TEntity>.Span_get_Length);
                il.Emit(OpCodes.Blt, forStart);
            }
        }

        var startIndex = il.DeclareLocal(typeof(int));
        // using(var reader = command.ExecuteReader())
        {
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(DbCommand_ExecuteReader);
            il.EmitStloc(reader);
            il.BeginExceptionBlock();
            {
                // reader.Read()
                il.EmitLdloc(reader);
                il.EmitCall(DbDataReader_Read);
                il.Emit(OpCodes.Pop);

                // var startIndex = reader.GetInt32(0)
                il.EmitLdloc(reader);
                il.EmitLdc_I4(0);
                il.EmitCall(DbDataReader_GetInt32);
                il.EmitStloc(startIndex);
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

        // for(var i = 0 ; i < entities.Length; i++)
        {
            entity = il.DeclareLocal(typeof(TEntity));
            var forStart = il.DefineLabel();
            var forEnd = il.DefineLabel();
            il.EmitLdc_I4(0);
            il.EmitStloc(i);
            il.Emit(OpCodes.Br, forEnd);
            {
                il.MarkLabel(forStart);

                // entities[i].Id = startIndex + i;
                il.EmitLdarga_S(1);
                il.EmitLdloc(i);
                il.EmitCall(Cast<TEntity>.Span_get_Item);
                il.Emit(OpCodes.Ldind_Ref);
                il.EmitLdloc(startIndex);
                il.EmitLdloc(i);
                il.Emit(OpCodes.Add);
                il.EmitCall(meta.Key.Info.GetSetMethod()!);

                // i++
                il.EmitLdloc(i);
                il.EmitLdc_I4(1);
                il.Emit(OpCodes.Add);
                il.EmitStloc(i);

                // foreach end
                il.MarkLabel(forEnd);
                il.EmitLdloc(i);
                il.EmitLdarga_S(1);
                il.EmitCall(Cast<TEntity>.Span_get_Length);
                il.Emit(OpCodes.Blt, forStart);
            }
        }

        il.Emit(OpCodes.Ret);

#if SaveDynamicAssmembly
        // Compile
        var type2 = type.CreateType();
        save($@"{Environment.CurrentDirectory}\{_assemblyName}.dll", type2);

        return (ExecuteUpdate<TEntity>)Delegate.CreateDelegate(typeof(ExecuteUpdate<TEntity>), type2.GetMethod(_methodName)!);
#else
        return method.CreateDelegate<ExecuteUpdate<TEntity>>();
#endif
    }

    void emitAddParameters(ILGenerator il, LocalBuilder entity, LocalBuilder index)
    {
        var meta = _metaService.GetEntityMeta<TEntity>();
        for (var i = 0; i < meta.Properties.Count; i++)
        {
            var property = meta.Properties[i];
            if (property.IsAutoIncrement)
                continue;

            // command.Parameter[index++].Value = Entity.XXX
            il.Emit(OpCodes.Ldarg_0);
            il.EmitCall(DbCommand_get_Parameters);
            il.EmitLdloc(index);
            il.Emit(OpCodes.Dup);
            il.EmitLdc_I4(1);
            il.Emit(OpCodes.Add);
            il.EmitStloc(index);
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
                if (property.Info.IsNullableReference())
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
