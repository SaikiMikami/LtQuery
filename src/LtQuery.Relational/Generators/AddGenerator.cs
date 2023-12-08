using LtQuery.Metadata;
using System.Data.Common;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

class AddGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    public AddGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }

    const string _methodName = "__ExecuteAdd";
    static int _no = 0;
    public ExecuteAdd<TEntity> CreateInjectParametersFunc<TEntity>() where TEntity : class
    {
        var methodb = new DynamicMethod($"{_methodName}_{_no++}", null, new[] { typeof(DbDataReader), typeof(Span<TEntity>) }, GetType().Module, true);

        var il = methodb.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();
        var setIdMethod = meta.Key.Info.GetSetMethod()!;
        var index = il.DeclareLocal(typeof(int));

        // var index = 0
        il.EmitLdc_I4(0);
        il.EmitStloc(index);

        // while(reader.Read()) Start
        var whileStartLabel = il.DefineLabel();
        var whileEndLabel = il.DefineLabel();
        il.Emit(OpCodes.Br_S, whileEndLabel);
        {
            il.MarkLabel(whileStartLabel);

            // entities[index++].Id = reader.GetInt32(0)
            il.Emit(OpCodes.Ldarga_S, 1);
            il.EmitLdloc(index);
            il.Emit(OpCodes.Dup);
            il.EmitLdc_I4(1);
            il.Emit(OpCodes.Add);
            il.EmitStloc(index);
            il.EmitCall(Cast<TEntity>.Span_get_Item);
            il.Emit(OpCodes.Ldind_Ref);
            il.EmitLdarg(0);
            il.EmitLdc_I4(0);
            il.EmitCall(DbDataReader_GetInt32);
            il.EmitCall(setIdMethod);

            // while (reader.Read()) End
            il.MarkLabel(whileEndLabel);
            il.EmitLdarg(0);
            il.EmitCall(DbDataReader_Read);
            il.Emit(OpCodes.Brtrue_S, whileStartLabel);
        }

        il.Emit(OpCodes.Ret);

        return methodb.CreateDelegate<ExecuteAdd<TEntity>>();
    }
}
