using LtQuery.Metadata;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace LtQuery.Relational.Generators;

class InjectParameterGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    public InjectParameterGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "__InjectParameter";
    static int _no = 0;
    public InjectParameter<TParameter> CreateInjectParameterFunc<TParameter>()
    {
        var methodb = new DynamicMethod($"{_methodName}_{_no++}", null, new[] { typeof(DbCommand), typeof(TParameter) }, GetType().Module, true);

        var il = methodb.GetILGenerator();

        var properties = typeof(TParameter).GetProperties();

        var p = il.DeclareLocal(typeof(DbParameter));

        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i];

            //var p = command.CreateParameter()
            il.EmitLdarg(0);
            il.EmitCall(DbCommand_CreateParameter);
            il.EmitStloc(p);

            // p.ParameterName = "Name";
            il.EmitLdloc(p);
            il.Emit(OpCodes.Ldstr, $"@{property.Name}");
            il.EmitCall(DbParameter_set_ParameterName);

            // p.DbType = dbType;
            il.EmitLdloc(p);
            il.EmitLdc_I4((int)getDbType(property.PropertyType));

            il.EmitCall(DbParameter_set_DbType);

            // p.Value = arg.XXX
            {
                il.EmitLdloc(p);
                il.EmitLdarg(1);
                il.EmitCall(property.GetGetMethod()!);

                var parameterType = properties[i].PropertyType;
                if (parameterType.IsValueType)
                    il.Emit(OpCodes.Box, parameterType);

                // ?? DbNull.Value
                if (property.IsNullableReference() != false)
                {
                    var label = il.DefineLabel();
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Brtrue_S, label);
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldsfld, DBNullType_Value);
                    il.MarkLabel(label);
                }

                il.EmitCall(DbParameter_set_Value);
            }

            // command.Parameters.Add(p);
            il.EmitLdarg(0);
            il.EmitCall(DbCommand_get_Parameters);
            il.EmitLdloc(p);
            il.EmitCall(DbParameterCollection_Add);
            il.Emit(OpCodes.Pop);
        }
        il.Emit(OpCodes.Ret);

        return methodb.CreateDelegate<InjectParameter<TParameter>>();
    }



    static MethodInfo _DefaultInterpolatedStringHandler_AppendFormattedInt = typeof(DefaultInterpolatedStringHandler).GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(_ => _.Name == nameof(DefaultInterpolatedStringHandler.AppendFormatted) && _.IsGenericMethod && _.GetParameters().Length == 1).MakeGenericMethod(typeof(int));


    public InjectParameterForUpdate<TEntity> CreateInjectParameterForUpdateFunc<TEntity>(DbMethod dbMethod) where TEntity : class
    {
        var methodb = new DynamicMethod($"{_methodName}_{_no++}", null, new[] { typeof(DbCommand), typeof(Span<TEntity>) }, GetType().Module, true);

        var il = methodb.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();

        var p = il.DeclareLocal(typeof(DbParameter));

        var span = il.DeclareLocal(typeof(Span<TEntity>));
        var i = il.DeclareLocal(typeof(int));
        var entity = il.DeclareLocal(typeof(TEntity));
        var stringb = il.DeclareLocal(typeof(DefaultInterpolatedStringHandler));


        // span = arg1
        il.EmitLdarg(1);
        il.EmitStloc(span);
        // for(var i = 0; i < s.Length; i++)
        var forStart = il.DefineLabel();
        var forEnd = il.DefineLabel();
        il.EmitLdc_I4(0);
        il.EmitStloc(i);
        il.Emit(OpCodes.Br, forEnd);
        {
            il.MarkLabel(forStart);

            // entity = span[i]
            il.EmitLdloca_S(span);
            il.EmitLdloc(i);
            il.EmitCall(Cast<TEntity>.Span_get_Item);
            il.Emit(OpCodes.Ldind_Ref);
            il.EmitStloc(entity);

            var constructor = typeof(DefaultInterpolatedStringHandler).GetConstructor(new[] { typeof(int), typeof(int) })!;
            var method1 = _DefaultInterpolatedStringHandler_AppendFormattedInt;
            var method2 = typeof(DefaultInterpolatedStringHandler).GetMethod(nameof(DefaultInterpolatedStringHandler.AppendLiteral), new[] { typeof(string) })!;
            var method3 = typeof(DefaultInterpolatedStringHandler).GetMethod(nameof(DefaultInterpolatedStringHandler.ToStringAndClear))!;

            foreach (var property in meta.Properties)
            {
                switch (dbMethod)
                {
                    case DbMethod.Add:
                        if (property.IsAutoIncrement)
                            continue;
                        break;

                    case DbMethod.Remove:
                        if (!property.IsKey)
                            continue;
                        break;
                }

                var propertyType = property.Type;
                //var p = command.CreateParameter()
                il.EmitLdarg(0);
                il.EmitCall(DbCommand_CreateParameter);
                il.EmitStloc(p);

                // p.ParameterName = $"@{i}_Name";
                il.EmitLdloc(p);
                il.EmitLdloca_S(stringb);
                il.EmitLdc_I4(4);
                il.EmitLdc_I4(1);
                il.Emit(OpCodes.Call, constructor);
                il.EmitLdloca_S(stringb);
                il.Emit(OpCodes.Ldstr, "@");
                il.Emit(OpCodes.Call, method2);
                il.EmitLdloca_S(stringb);
                il.EmitLdloc(i);
                il.Emit(OpCodes.Call, method1);
                il.EmitLdloca_S(stringb);
                il.Emit(OpCodes.Ldstr, $"_{property.Name}");
                il.Emit(OpCodes.Call, method2);
                il.EmitLdloca_S(stringb);
                il.Emit(OpCodes.Call, method3);
                il.EmitCall(DbParameter_set_ParameterName);


                // p.DbType = getDbType(propertyType);
                il.EmitLdloc(p);
                il.EmitLdc_I4((int)getDbType(propertyType));

                il.EmitCall(DbParameter_set_DbType);

                // p.Value = entity.XXX
                {
                    il.EmitLdloc(p);
                    il.EmitLdloc(entity);
                    il.EmitCall(property.Info.GetGetMethod()!);

                    if (propertyType.IsValueType)
                        il.Emit(OpCodes.Box, propertyType);

                    // ?? DbNull.Value
                    if (property.Info.IsNullableReference() != false)
                    {
                        var label = il.DefineLabel();
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Brtrue_S, label);
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldsfld, DBNullType_Value);
                        il.MarkLabel(label);
                    }

                    il.EmitCall(DbParameter_set_Value);
                }

                // command.Parameters.Add(p);
                il.EmitLdarg(0);
                il.EmitCall(DbCommand_get_Parameters);
                il.EmitLdloc(p);
                il.EmitCall(DbParameterCollection_Add);
                il.Emit(OpCodes.Pop);
            }

            // i++
            il.EmitLdloc(i);
            il.EmitLdc_I4(1);
            il.Emit(OpCodes.Add);
            il.EmitStloc(i);
            // i < span.Length
            il.MarkLabel(forEnd);
            il.EmitLdloc(i);
            il.EmitLdloca_S(span);
            il.EmitCall(Cast<TEntity>.Span_get_Length);
            il.Emit(OpCodes.Blt, forStart);
        }
        il.Emit(OpCodes.Ret);

        return methodb.CreateDelegate<InjectParameterForUpdate<TEntity>>();
    }

    static DbType getDbType(Type type)
    {
        if (type == typeof(int) || type == typeof(int?))
            return DbType.Int32;
        else if (type == typeof(long) || type == typeof(long?))
            return DbType.Int64;
        else if (type == typeof(short) || type == typeof(short?))
            return DbType.Int16;
        else if (type == typeof(decimal) || type == typeof(decimal?))
            return DbType.Decimal;
        else if (type == typeof(byte) || type == typeof(byte?))
            return DbType.Byte;
        else if (type == typeof(bool) || type == typeof(bool?))
            return DbType.Boolean;
        else if (type == typeof(Guid) || type == typeof(Guid?))
            return DbType.Guid;
        else if (type == typeof(DateTime) || type == typeof(DateTime?))
            return DbType.DateTime;
        else if (type == typeof(string))
            return DbType.String;
        else
            throw new NotSupportedException();
    }
}
