using LtQuery.Elements;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

class QueryTree
{
    public TableTree TopTable { get; }
    public int ReaderIndex { get; }
    public QueryTree(QueryTree? parent, TableTree? parentTable, QueryNode current, ref int readerIndex)
    {
        Parent = parent;
        ReaderIndex = readerIndex++;
        TopTable = new TableTree(this, parentTable, current);
        foreach (var child in current.Children)
        {
            if (child.Navigation!.IsSplited)
            {
                var childQuery = new QueryTree(this, TopTable, child, ref readerIndex);
                Children.Add(childQuery);
            }
        }
    }
    public QueryTree(QueryNode current, IBoolValue? condition, IValue? skipCount, IValue? takeCount, ref int readerIndex) : this(null, null, current, ref readerIndex) { }

    public QueryTree? Parent { get; }
    public List<QueryTree> Children { get; } = new();


    public void EmitSelect(ILGenerator il)
    {
        TopTable.CreateLocalAndLabel(il);

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
            il.EmitLdloc(1);
            il.EmitCall(typeof(IDataReader).GetMethod("Read")!);
            il.Emit(OpCodes.Brtrue, whileStartLabel);
        }

        foreach (var child in Children)
        {
            il.EmitLdloc(1);
            il.EmitCall(typeof(DbDataReader).GetMethod("NextResult")!);
            il.Emit(OpCodes.Pop);

            child.EmitSelect(il);
        }
    }
}