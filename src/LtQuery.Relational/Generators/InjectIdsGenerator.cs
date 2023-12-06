using LtQuery.Metadata;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

class InjectIdsGenerator : AbstractGenerator
{
    readonly EntityMetaService _metaService;
    public InjectIdsGenerator(EntityMetaService metaService)
    {
        _metaService = metaService;
    }
    const string _methodName = "__InjectId";
    static int _no = 0;
    public InjectIds<TEntity> CreateInjectParametersFunc<TEntity>() where TEntity : class
    {
        var methodb = new DynamicMethod($"{_methodName}_{_no++}", null, new[] { typeof(Span<TEntity>), typeof(Span<int>) }, GetType().Module, true);

        var il = methodb.GetILGenerator();

        var meta = _metaService.GetEntityMeta<TEntity>();
        var setIdMethod = meta.Key.Info.GetSetMethod()!;

        var entities = il.DeclareLocal(typeof(Span<TEntity>));
        var ids = il.DeclareLocal(typeof(Span<int>));
        var i = il.DeclareLocal(typeof(int));

        // entities = arg0
        il.EmitLdarg(0);
        il.EmitStloc(entities);

        // ids = arg1
        il.EmitLdarg(1);
        il.EmitStloc(ids);

        // for(var i = 0; i < s.Length; i++)
        var forStart = il.DefineLabel();
        var forEnd = il.DefineLabel();
        il.EmitLdc_I4(0);
        il.EmitStloc(i);
        il.Emit(OpCodes.Br_S, forEnd);
        {
            il.MarkLabel(forStart);

            // entities[i].Id = ids[i]
            il.EmitLdloca_S(entities);
            il.EmitLdloc(i);
            il.EmitCall(Cast<TEntity>.Span_get_Item);
            il.Emit(OpCodes.Ldind_Ref);
            il.EmitLdloca_S(ids);
            il.EmitLdloc(i);
            il.EmitCall(Cast<int>.Span_get_Item);
            il.Emit(OpCodes.Ldind_I4);
            il.EmitCall(setIdMethod);

            // i++
            il.EmitLdloc(i);
            il.EmitLdc_I4(1);
            il.Emit(OpCodes.Add);
            il.EmitStloc(i);
            // i < ids.Length
            il.MarkLabel(forEnd);
            il.EmitLdloc(i);
            il.EmitLdloca_S(ids);
            il.EmitCall(Cast<int>.Span_get_Length);
            il.Emit(OpCodes.Blt_S, forStart);
        }

        il.Emit(OpCodes.Ret);

        return methodb.CreateDelegate<InjectIds<TEntity>>();
    }
}
