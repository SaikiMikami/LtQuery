using System.Linq.Expressions;

namespace LtQuery.Fluents
{
    class ReplaceVisitor : ExpressionVisitor
    {
        readonly MemberExpression _source;
        public ReplaceVisitor(MemberExpression source)
        {
            _source = source;
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {

            return _source;
        }
    }
}
