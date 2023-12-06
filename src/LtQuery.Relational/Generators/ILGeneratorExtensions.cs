using System.Reflection;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

public static class ILGeneratorExtensions
{
    public static void EmitLdarg(this ILGenerator _this, int num)
    {
        switch (num)
        {
            case 0:
                _this.Emit(OpCodes.Ldarg_0);
                break;
            case 1:
                _this.Emit(OpCodes.Ldarg_1);
                break;
            case 2:
                _this.Emit(OpCodes.Ldarg_2);
                break;
            case 3:
                _this.Emit(OpCodes.Ldarg_3);
                break;
            default:
                _this.Emit(OpCodes.Ldarg_S, (sbyte)num);
                break;
        }
    }
    public static void EmitLdarga_S(this ILGenerator _this, int num)
        => _this.Emit(OpCodes.Ldarga_S, (sbyte)num);

    public static void EmitLdloc(this ILGenerator _this, LocalBuilder local) => _this.EmitLdloc(local.LocalIndex);
    public static void EmitLdloc(this ILGenerator _this, int num)
    {
        switch (num)
        {
            case 0:
                _this.Emit(OpCodes.Ldloc_0);
                break;
            case 1:
                _this.Emit(OpCodes.Ldloc_1);
                break;
            case 2:
                _this.Emit(OpCodes.Ldloc_2);
                break;
            case 3:
                _this.Emit(OpCodes.Ldloc_3);
                break;
            default:
                _this.Emit(OpCodes.Ldloc_S, (sbyte)num);
                break;
        }
    }

    public static void EmitLdloca_S(this ILGenerator _this, LocalBuilder local) => _this.EmitLdloca_S(local.LocalIndex);
    public static void EmitLdloca_S(this ILGenerator _this, int num)
        => _this.Emit(OpCodes.Ldloca_S, (sbyte)num);

    public static void EmitStloc(this ILGenerator _this, LocalBuilder local) => _this.EmitStloc(local.LocalIndex);
    public static void EmitStloc(this ILGenerator _this, int num)
    {
        switch (num)
        {
            case 0:
                _this.Emit(OpCodes.Stloc_0);
                break;
            case 1:
                _this.Emit(OpCodes.Stloc_1);
                break;
            case 2:
                _this.Emit(OpCodes.Stloc_2);
                break;
            case 3:
                _this.Emit(OpCodes.Stloc_3);
                break;
            default:
                _this.Emit(OpCodes.Stloc_S, (sbyte)num);
                break;
        }
    }

    public static void EmitLdfld(this ILGenerator _this, FieldBuilder field)
        => _this.Emit(OpCodes.Ldfld, field);

    public static void EmitStfld(this ILGenerator _this, FieldBuilder field)
        => _this.Emit(OpCodes.Stfld, field);

    public static void EmitLdc_I4(this ILGenerator _this, int num)
    {
        switch (num)
        {
            case 0:
                _this.Emit(OpCodes.Ldc_I4_0);
                break;
            case 1:
                _this.Emit(OpCodes.Ldc_I4_1);
                break;
            case 2:
                _this.Emit(OpCodes.Ldc_I4_2);
                break;
            case 3:
                _this.Emit(OpCodes.Ldc_I4_3);
                break;
            case 4:
                _this.Emit(OpCodes.Ldc_I4_4);
                break;
            case 5:
                _this.Emit(OpCodes.Ldc_I4_5);
                break;
            case 6:
                _this.Emit(OpCodes.Ldc_I4_6);
                break;
            case 7:
                _this.Emit(OpCodes.Ldc_I4_7);
                break;
            case 8:
                _this.Emit(OpCodes.Ldc_I4_8);
                break;
            default:
                _this.Emit(OpCodes.Ldc_I4_S, (sbyte)num);
                break;
        }
    }

    public static void EmitCall(this ILGenerator _this, MethodInfo methodInfo)
    {
        if (methodInfo.IsFinal || !methodInfo.IsVirtual)
            _this.Emit(OpCodes.Call, methodInfo);
        else
            _this.Emit(OpCodes.Callvirt, methodInfo);
    }

    public static void EmitCastOrUnbox_Any(this ILGenerator _this, Type type)
    {
        if (type.IsValueType)
            _this.Emit(OpCodes.Unbox_Any, type);
        else
            _this.Emit(OpCodes.Castclass, type);
    }
}
