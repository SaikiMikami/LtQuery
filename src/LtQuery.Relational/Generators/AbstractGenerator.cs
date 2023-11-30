using System.Collections;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Emit;

namespace LtQuery.Relational.Generators
{
    public abstract class AbstractGenerator
    {
        protected static readonly Type IDisposableType = typeof(IDisposable);
        protected static readonly MethodInfo IDisposable_Dispose = IDisposableType.GetMethod("Dispose")!;

        public static readonly Type IEnumeratorType = typeof(IEnumerator);
        public static readonly MethodInfo IEnumerator_MoveNext = IEnumeratorType.GetMethod("MoveNext")!;

        public static readonly Type DBNullType = typeof(DBNull);
        public static readonly FieldInfo DBNullType_Value = DBNullType.GetField("Value")!;

        protected static readonly Type DbCommandType = typeof(DbCommand);
        protected static readonly MethodInfo DbCommand_get_Parameters = DbCommandType.GetProperty("Parameters")!.GetGetMethod()!;
        protected static readonly MethodInfo DbCommand_ExecuteReader = DbCommandType.GetMethod("ExecuteReader", Array.Empty<Type>())!;

        protected static readonly Type DbParameterCollectionType = typeof(DbParameterCollection);
        protected static readonly MethodInfo DbParameterCollection_get_Item = DbParameterCollectionType.GetProperty("Item", new Type[] { typeof(int) })!.GetGetMethod()!;

        protected static readonly Type DbParameterType = typeof(DbParameter);
        protected static readonly MethodInfo DbParameter_set_Value = DbParameterType.GetProperty("Value")!.GetSetMethod()!;

        protected static readonly Type DbDataReaderType = typeof(DbDataReader);
        protected static readonly MethodInfo DbDataReader_Read = DbDataReaderType.GetMethod("Read")!;
        protected static readonly MethodInfo DbDataReader_IsDBNull = DbDataReaderType.GetMethod(nameof(DbDataReader.IsDBNull))!;
        protected static readonly MethodInfo DbDataReader_GetInt32 = DbDataReaderType.GetMethod(nameof(DbDataReader.GetInt32))!;
        protected static readonly MethodInfo DbDataReader_GetInt64 = DbDataReaderType.GetMethod(nameof(DbDataReader.GetInt64))!;
        protected static readonly MethodInfo DbDataReader_GetInt16 = DbDataReaderType.GetMethod(nameof(DbDataReader.GetInt16))!;
        protected static readonly MethodInfo DbDataReader_GetDecimal = DbDataReaderType.GetMethod(nameof(DbDataReader.GetDecimal))!;
        protected static readonly MethodInfo DbDataReader_GetByte = DbDataReaderType.GetMethod(nameof(DbDataReader.GetByte))!;
        protected static readonly MethodInfo DbDataReader_GetBoolean = DbDataReaderType.GetMethod(nameof(DbDataReader.GetBoolean))!;
        protected static readonly MethodInfo DbDataReader_GetGuid = DbDataReaderType.GetMethod(nameof(DbDataReader.GetGuid))!;
        protected static readonly MethodInfo DbDataReader_GetDateTime = DbDataReaderType.GetMethod(nameof(DbDataReader.GetDateTime))!;
        protected static readonly MethodInfo DbDataReader_GetString = DbDataReaderType.GetMethod(nameof(DbDataReader.GetString))!;

        protected class Cast<TEntity>
        {
            public static readonly Type Type = typeof(TEntity);

            public static readonly Type IEnumerableType = typeof(IEnumerable<TEntity>);
            public static readonly MethodInfo IEnumerable_GetEnumerator = IEnumerableType.GetMethod("GetEnumerator")!;

            public static readonly Type IEnumeratorType = typeof(IEnumerator<TEntity>);
            public static readonly MethodInfo IEnumerator_get_Current = IEnumeratorType.GetProperty("Current")!.GetGetMethod()!;

            public static readonly Type SpanType = typeof(Span<TEntity>);
            public static readonly MethodInfo Span_get_Item = SpanType.GetProperty("Item")!.GetGetMethod()!;
            public static readonly MethodInfo Span_get_Length = SpanType.GetProperty("Length")!.GetGetMethod()!;


            public static readonly Type IReadOnlyCollectionType = typeof(IReadOnlyCollection<TEntity>);
            public static readonly MethodInfo IReadOnlyCollectionType_get_Count = IReadOnlyCollectionType.GetProperty("Count")!.GetGetMethod()!;


            public static readonly Type IReadOnlyListType = typeof(IReadOnlyList<TEntity>);
            public static readonly MethodInfo IReadOnlyList_get_Item = IReadOnlyListType.GetProperty("Item")!.GetGetMethod()!;
        }

        protected static void EmitCallReader_Read(ILGenerator il, LocalBuilder reader, Type type, int index)
        {
            il.EmitLdloc(reader);
            il.EmitLdc_I4(index);
            if (type == typeof(int))
                il.EmitCall(DbDataReader_GetInt32);
            else if (type == typeof(long))
                il.EmitCall(DbDataReader_GetInt64);
            else if (type == typeof(short))
                il.EmitCall(DbDataReader_GetInt16);
            else if (type == typeof(decimal))
                il.EmitCall(DbDataReader_GetDecimal);
            else if (type == typeof(byte))
                il.EmitCall(DbDataReader_GetByte);
            else if (type == typeof(bool))
                il.EmitCall(DbDataReader_GetBoolean);
            else if (type == typeof(Guid))
                il.EmitCall(DbDataReader_GetGuid);
            else if (type == typeof(DateTime))
                il.EmitCall(DbDataReader_GetDateTime);
            else if (type == typeof(string))
                il.EmitCall(DbDataReader_GetString);
            else
                throw new NotSupportedException();
        }
    }
}
