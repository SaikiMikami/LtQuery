using System.Data.Common;

namespace LtQuery.Relational;

delegate void InjectParameter<TParameter>(DbCommand command, TParameter parameter);
delegate void InjectParameterForUpdate<TEntity>(DbCommand command, Span<TEntity> entities);

delegate IReadOnlyList<TEntity> ExecuteSelect<TEntity>(DbDataReader reader) where TEntity : class;
delegate void ExecuteAdd<TEntity>(DbDataReader reader, Span<TEntity> entities);
