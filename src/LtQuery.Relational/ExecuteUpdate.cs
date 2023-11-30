using System.Data.Common;

namespace LtQuery.Relational;

public delegate void ExecuteUpdate<TEntity>(DbCommand command, Span<TEntity> entities);
