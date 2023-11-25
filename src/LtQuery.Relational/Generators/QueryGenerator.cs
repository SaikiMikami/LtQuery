using LtQuery.Relational.Nodes;
using System.Data.Common;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators;

class QueryGenerator
{
    public QueryNode Query { get; }
    public TableGenerator RootTable { get; }
    public QueryGenerator? Parent => ParentTable?.QueryGenerator;
    public TableGenerator? ParentTable { get; }
    public IReadOnlyList<QueryGenerator> Children { get; }
    public QueryGenerator(TableGenerator? parentTable, QueryNode query)
    {
        ParentTable = parentTable;
        Query = query;
        RootTable = new TableGenerator(parentTable, this, Query.RootTable);
        var list = new List<QueryGenerator>();
        foreach (var child in query.Children)
        {
            parentTable = RootTable.Search(child.ParentTable!) ?? throw new InvalidProgramException();
            list.Add(new(parentTable, child));
        }
        Children = list;
    }

    public void EmitSelect(ILGenerator il)
    {
        var topTable = RootTable;
        topTable.CreateLocalAndLabel(il);

        topTable.EmitInit(il);

        // while(reader.Read()) Start
        var whileStartLabel = il.DefineLabel();
        var whileEndLabel = il.DefineLabel();
        il.Emit(OpCodes.Br, whileEndLabel);
        {
            il.MarkLabel(whileStartLabel);

            var index = 0;
            topTable.EmitCreate(il, ref index);

            // while (reader.Read()) End
            il.MarkLabel(whileEndLabel);
            il.EmitLdloc(1);
            il.EmitCall(typeof(DbDataReader).GetMethod("Read")!);
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

    public TableGenerator? SearchTable(Type type)
    {
        var table = RootTable.Search(type);
        if (table != null)
            return table;

        if (Parent != null)
        {
            table = Parent.SearchTable(type);
            if (table != null)
                return table;
        }
        return null;
    }

    public bool HasSubQuery() => Query.Children.Count != 0;
}
