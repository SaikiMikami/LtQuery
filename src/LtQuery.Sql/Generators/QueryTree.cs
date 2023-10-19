using LtQuery.Elements;
using LtQuery.Elements.Values;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;

namespace LtQuery.Sql.Generators;

class QueryTree
{
    public TableTree TopTable { get; }
    public IReadOnlyList<ParameterValue> Parameters { get; }
    public int ReaderIndex { get; }
    public QueryTree(QueryTree? parent, TableTree? parentTable, QueryNode current, IBoolValue? condition, IValue? skipCount, IValue? takeCount, ref int readerIndex)
    {
        Parent = parent;
        ReaderIndex = readerIndex++;
        TopTable = new TableTree(this, parentTable, current);
        foreach (var child in current.Children)
        {
            if (child.Navigation!.IsSplited)
            {
                var childQuery = new QueryTree(this, TopTable, child, condition, skipCount, takeCount, ref readerIndex);
                Children.Add(childQuery);
            }
        }

        var parameters = new List<ParameterValue>();
        if (condition != null)
        {
            buildParameterValues(parameters, condition);
        }
        if (skipCount != null)
        {
            buildParameterValues(parameters, skipCount);
        }
        if (takeCount != null)
        {
            buildParameterValues(parameters, takeCount);
        }
        Parameters = parameters;
    }
    public QueryTree(QueryNode current, IBoolValue? condition, IValue? skipCount, IValue? takeCount, ref int readerIndex) : this(null, null, current, condition, skipCount, takeCount, ref readerIndex) { }

    public QueryTree? Parent { get; }
    public List<QueryTree> Children { get; } = new();

    public LocalBuilder? Command { get; private set; }
    public LocalBuilder? Reader { get; private set; }


    public void EmitSelect(ILGenerator il)
    {
        TopTable.CreateLocalAndLabel(il);

        // using(var reader = command.ExecuteReader())
        Reader = il.DeclareLocal(typeof(DbDataReader));
        var finallyEnd = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdc_I4(ReaderIndex);
        il.EmitCall(typeof(IReadOnlyList<DbCommand>).GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
        il.EmitCall(typeof(DbCommand).GetMethod("ExecuteReader", Array.Empty<Type>())!);
        il.EmitStloc(Reader);
        var tryStart = il.BeginExceptionBlock();
        {
            TopTable.EmitInit(il);

            // while(reader.Read()) Start
            var whileStartLabel = il.DefineLabel();
            var whileEndLabel = il.DefineLabel();
            il.Emit(OpCodes.Br, whileEndLabel);
            {
                il.MarkLabel(whileStartLabel);

                var index = 0;
                TopTable.EmitCreate(il, ref index);

                // while (reader.Read()) End
                il.MarkLabel(whileEndLabel);
                il.EmitLdloc(Reader);
                il.EmitCall(typeof(IDataReader).GetMethod("Read")!);
                il.Emit(OpCodes.Brtrue, whileStartLabel);
            }

            il.Emit(OpCodes.Leave_S, tryStart);
        }
        // finally
        {
            il.BeginFinallyBlock();
            il.EmitLdloc(Reader);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(Reader);
            il.EmitCall(typeof(IDisposable).GetMethod("Dispose")!);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();

            //il.Emit(OpCodes.Endfinally);
        }


        foreach (var child in Children)
        {
            child.EmitSelect(il);
        }
    }

    public void EmitSelect<TParameter>(ILGenerator il)
    {
        var type = TopTable.Type;
        TopTable.CreateLocalAndLabel(il);

        // command.Parameters[i].Value = 
        Command = il.DeclareLocal(typeof(DbCommand));
        Reader = il.DeclareLocal(typeof(DbDataReader));
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdc_I4(ReaderIndex);
        il.EmitCall(typeof(IReadOnlyList<DbCommand>).GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
        il.EmitStloc(Command);

        for (var i = 0; i < Parameters.Count; i++)
        {
            il.EmitLdloc(Command);
            il.EmitCall(typeof(DbCommand).GetProperty("Parameters")!.GetGetMethod()!);
            il.EmitLdc_I4(i);
            il.EmitCall(typeof(DbParameterCollection).GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
            il.Emit(OpCodes.Ldarg_1);
            var method = typeof(TParameter).GetProperty(Parameters[i].Name)?.GetGetMethod() ?? throw new InvalidOperationException($"Argument [{Parameters[i].Name}] is not included in the query");
            il.EmitCall(method);
            var parameterType = Parameters[i].Type;
            if (parameterType.IsValueType)
                il.Emit(OpCodes.Box, parameterType);
            il.EmitCall(typeof(DbParameter).GetProperty("Value")!.GetSetMethod()!);
        }

        // using(var reader = command.ExecuteReader())
        var finallyEnd = il.DefineLabel();

        il.EmitLdloc(Command);
        il.EmitCall(typeof(DbCommand).GetMethod("ExecuteReader", Array.Empty<Type>())!);
        il.EmitStloc(Reader);
        var tryStart = il.BeginExceptionBlock();
        {
            TopTable.EmitInit(il);

            // while(reader.Read()) Start
            var whileStartLabel = il.DefineLabel();
            var whileEndLabel = il.DefineLabel();
            il.Emit(OpCodes.Br, whileEndLabel);
            {
                il.MarkLabel(whileStartLabel);

                var index = 0;
                TopTable.EmitCreate(il, ref index);

                // while (reader.Read()) End
                il.MarkLabel(whileEndLabel);
                il.EmitLdloc(Reader);
                il.EmitCall(typeof(IDataReader).GetMethod("Read")!);
                il.Emit(OpCodes.Brtrue, whileStartLabel);
            }

            il.Emit(OpCodes.Leave_S, tryStart);
        }
        // finally
        {
            il.BeginFinallyBlock();
            il.EmitLdloc(Reader);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(Reader);
            il.EmitCall(typeof(IDisposable).GetMethod("Dispose")!);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();

            //il.Emit(OpCodes.Endfinally);
        }


        foreach (var child in Children)
        {
            child.EmitSelect<TParameter>(il);
        }
    }

    public void EmitFirst(ILGenerator il)
    {
        TopTable.CreateLocalAndLabel(il);

        // using(var reader = command.ExecuteReader())
        Reader = il.DeclareLocal(typeof(DbDataReader));
        var finallyEnd = il.DefineLabel();
        il.Emit(OpCodes.Ldarg_0);
        il.EmitLdc_I4(ReaderIndex);
        il.EmitCall(typeof(IReadOnlyList<DbCommand>).GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!);
        il.EmitCall(typeof(DbCommand).GetMethod("ExecuteReader", Array.Empty<Type>())!);
        il.EmitStloc(Reader);
        var tryStart = il.BeginExceptionBlock();
        {
            TopTable.EmitInit(il);

            // while(reader.Read()) Start
            var whileStartLabel = il.DefineLabel();
            var whileEndLabel = il.DefineLabel();
            il.Emit(OpCodes.Br, whileEndLabel);
            {
                il.MarkLabel(whileStartLabel);

                var index = 0;
                TopTable.EmitCreate(il, ref index);

                // while (reader.Read()) End
                il.MarkLabel(whileEndLabel);
                il.EmitLdloc(Reader);
                il.EmitCall(typeof(IDataReader).GetMethod("Read")!);
                il.Emit(OpCodes.Brtrue, whileStartLabel);
            }

            il.Emit(OpCodes.Leave_S, tryStart);
        }
        // finally
        {
            il.BeginFinallyBlock();
            il.EmitLdloc(Reader);
            il.Emit(OpCodes.Brfalse_S, finallyEnd);
            il.EmitLdloc(Reader);
            il.EmitCall(typeof(IDisposable).GetMethod("Dispose")!);
            il.MarkLabel(finallyEnd);
            il.EndExceptionBlock();

            //il.Emit(OpCodes.Endfinally);
        }


        foreach (var child in Children)
        {
            child.EmitSelect(il);
        }
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
}