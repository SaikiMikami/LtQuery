using System.Data.Common;

namespace LtQuery.Relational;

delegate void InjectParameter<TParameter>(DbCommand command, TParameter parameter);
delegate void InjectParameterForUpdate<TEntity>(DbCommand command, Span<TEntity> entities);

delegate IReadOnlyList<TEntity> ExecuteSelect<TEntity>(DbCommand command) where TEntity : class;
delegate ValueTask<IReadOnlyList<TEntity>> ExecuteSelectAsync<TEntity>(DbCommand command, CancellationToken cancellationToken = default) where TEntity : class;

delegate void InjectIds<TEntity>(Span<TEntity> entities, Span<int> ids);
