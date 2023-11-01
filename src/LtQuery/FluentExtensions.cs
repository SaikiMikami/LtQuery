using LtQuery.Elements;
using LtQuery.Elements.Values;
using LtQuery.Elements.Values.Operators;
using LtQuery.Fluents;
using System.Linq.Expressions;
using System.Reflection;

namespace LtQuery;

public static class FluentExtensions
{

    #region IQueryFluent

    public static IQueryFluent<TEntity> Where<TEntity>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        var body = predicate.Body;

        var op = convertToValue(body);
        if (op is not IBoolValue)
            throw new InvalidOperationException("Arg of Where() must is boolean type");
        return _this.Where((IBoolValue)op);
    }

    public static IQueryFluent<TEntity> Skip<TEntity>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, int>> count) where TEntity : class
    {
        switch (count.Body)
        {
            case ConstantExpression constant:
                var o = constant.Value;
                if (o == null || o is not int)
                    throw new ArgumentException("Must be int type", nameof(count));
                return _this.Skip((int)o);
            case MemberExpression member:
                throw new ArgumentException("Can't pass variables with LINQ", nameof(count));
            case MethodCallExpression call:
                if (!isArg(call.Method))
                    throw new ArgumentException();
                var parameter = getParameter(call);
                return _this.Skip(parameter.Name);
            default:
                throw new ArgumentException();
        }
    }

    public static IQueryFluent<TEntity> Take<TEntity>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, int>> count) where TEntity : class
    {
        switch (count.Body)
        {
            case ConstantExpression constant:
                var o = constant.Value;
                if (o == null || o is not int)
                    throw new ArgumentException("Must be int type", nameof(count));
                return _this.Take((int)o);
            case MemberExpression member:
                throw new ArgumentException("Can't pass variables with LINQ", nameof(count));
            case MethodCallExpression call:
                if (!isArg(call.Method))
                    throw new ArgumentException();
                var parameter = getParameter(call);
                return _this.Take(parameter.Name);
            default:
                throw new ArgumentException();
        }
    }

    public static IQueryAndOrderByFluent<TEntity> OrderBy<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    {
        var exp = (MemberExpression)keySelector.Body;

        var list = new List<string>();
        while (exp != null)
        {
            list.Add(exp.Member.Name);
            exp = exp.Expression as MemberExpression;
        }
        list.Reverse();

        return _this.OrderBy(list);
    }

    public static IQueryAndOrderByFluent<TEntity> OrderByDescending<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    {
        var exp = (MemberExpression)keySelector.Body;

        var list = new List<string>();
        while (exp != null)
        {
            list.Add(exp.Member.Name);
            exp = exp.Expression as MemberExpression;
        }
        list.Reverse();

        return _this.OrderByDescending(list);
    }

    //public static IQueryAndOrderByFluent<TEntity> ThenBy<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    //{
    //    return _this;
    //}
    //public static IQueryAndOrderByFluent<TEntity> ThenByDescending<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    //{
    //    return _this;
    //}

    public static IQueryAndIncludeFluent<TEntity> Include<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TEntity : class
    {
        var exp = (MemberExpression)navigationPropertyPath.Body;

        var list = new List<string>();
        while (exp != null)
        {
            list.Add(exp.Member.Name);
            exp = exp.Expression as MemberExpression;
        }
        list.Reverse();

        return _this.Include(list);
    }

    //public static IQueryAndIncludeFluent<TEntity> ThenInclude<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TEntity : class
    //{
    //    return _this;
    //}

    #endregion

    #region ILtConnection

    public static int Count<TEntity>(this ILtConnection _this, QueryFluent<TEntity> query) where TEntity : class
        => _this.Count(query.ToImmutable());
    public static int Count<TEntity, TParameter>(this ILtConnection _this, QueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Count(query.ToImmutable(), values);

    public static IEnumerable<TEntity> Select<TEntity>(this ILtConnection _this, QueryFluent<TEntity> query) where TEntity : class
        => _this.Select(query.ToImmutable());
    public static IEnumerable<TEntity> Select<TEntity, TParameter>(this ILtConnection _this, QueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Select(query.ToImmutable(), values);

    public static TEntity Single<TEntity>(this ILtConnection _this, QueryFluent<TEntity> query) where TEntity : class
        => _this.Single(query.ToImmutable());
    public static TEntity Single<TEntity, TParameter>(this ILtConnection _this, QueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Single(query.ToImmutable(), values);

    public static TEntity First<TEntity>(this ILtConnection _this, QueryFluent<TEntity> query) where TEntity : class
        => _this.First(query.ToImmutable());
    public static TEntity First<TEntity, TParameter>(this ILtConnection _this, QueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.First(query.ToImmutable(), values);

    #endregion



    static IValue convertToValue(Expression exp)
    {
        switch (exp)
        {
            case BinaryExpression binary:
                var lhs = convertToValue(binary.Left);
                var rhs = convertToValue(binary.Right);
                switch (binary.NodeType)
                {
                    case ExpressionType.Equal:
                        return new EqualOperator(lhs, rhs);
                    case ExpressionType.NotEqual:
                        return new NotEqualOperator(lhs, rhs);
                    case ExpressionType.LessThan:
                        return new LessThanOperator(lhs, rhs);
                    case ExpressionType.LessThanOrEqual:
                        return new LessThanOrEqualOperator(lhs, rhs);
                    case ExpressionType.GreaterThan:
                        return new GreaterThanOperator(lhs, rhs);
                    case ExpressionType.GreaterThanOrEqual:
                        return new GreaterThanOrEqualOperator(lhs, rhs);
                    case ExpressionType.AndAlso:
                        return new AndAlsoOperator(lhs, rhs);
                    case ExpressionType.OrElse:
                        return new OrElseOperator(lhs, rhs);
                    default:
                        throw new NotSupportedException($"[{binary.NodeType}] is not supported");
                }

                throw new NotSupportedException($"not supported [{exp}]");
            case MemberExpression member:

                Expression? exp2 = member;
                var list = new List<string>();
                while (exp2 != null)
                {
                    switch (exp2)
                    {
                        case MemberExpression member2:
                            list.Add(member2.Member.Name);
                            exp2 = member2.Expression;
                            break;
                        case ParameterExpression:
                            exp2 = null;
                            break;
                        default:
                            throw new ArgumentException("Argument must be PropertyAccess", nameof(exp));
                    }
                }
                list.Reverse();

                return convertToProperty(list);
            case MethodCallExpression methodCall:
                var method = methodCall.Method;
                if (isArg(method))
                    return getParameter(methodCall);
                else if (isAny(method))
                {
                    var lhs2 = (MemberExpression)methodCall.Arguments[0];
                    var rhs2 = (LambdaExpression)methodCall.Arguments[1];

                    var a = convertToValue(lhs2);
                    var b = convertToValue(rhs2.Body);

                    // TODO support Any()
                    throw new NotSupportedException("Any() is not supported");
                }
                else
                    throw new ArgumentException("Method calls cannot be used", nameof(exp));
            case ConstantExpression constant:
                return new ConstantValue($"{constant.Value}");
            case UnaryExpression unary:
                switch (unary.NodeType)
                {
                    case ExpressionType.Convert:
                        var o = (ConstantExpression)unary.Operand;
                        return new ConstantValue(o.Value?.ToString());
                }
                throw new ArgumentException("Argument must be PropertyAccess", nameof(exp));
            default:
                throw new ArgumentException("Argument must be PropertyAccess", nameof(exp));
        }
    }

    static readonly MethodInfo _argMethod = typeof(Lt).GetMethod("Arg")!;

    static bool isArg(MethodInfo method) => method.GetGenericMethodDefinition() == _argMethod;

    static readonly MethodInfo _anyMethod = typeof(Enumerable).GetMethods().Single(_ => _.Name == "Any" && _.GetParameters().Length == 2)!;

    static bool isAny(MethodInfo method) => method.GetGenericMethodDefinition() == _anyMethod;

    static ParameterValue getParameter(MethodCallExpression exp)
    {
        var arg0 = exp.Arguments[0] as ConstantExpression ?? throw new ArgumentException("The first argument of Lt.Arg() method must be constant", nameof(exp));
        var name = (string?)arg0.Value ?? throw new ArgumentException("The first argument of Lt.Arg() method must not be null", nameof(exp));
        return new(name, exp.Type);
    }

    static PropertyValue convertToProperty(IReadOnlyList<string> propertyName) => convertToProperty(null, propertyName);
    static PropertyValue convertToProperty(PropertyValue? parent, IReadOnlyList<string> propertyName)
    {
        PropertyValue? value = parent;
        for (var i = 0; i < propertyName.Count; i++)
            value = new PropertyValue(value, propertyName[i]);
        return value ?? throw new ArgumentException();
    }
}
