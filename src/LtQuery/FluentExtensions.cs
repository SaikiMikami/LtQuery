﻿using LtQuery.Elements;
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
            case MemberExpression:
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
            case MemberExpression:
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
        var list = convertPropertyToStrings(keySelector);
        return _this.OrderBy(list);
    }

    public static IQueryAndOrderByFluent<TEntity> OrderByDescending<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    {
        var list = convertPropertyToStrings(keySelector);
        return _this.OrderByDescending(list);
    }

    public static IQueryAndOrderByFluent<TEntity> ThenBy<TEntity, TProperty>(this IQueryAndOrderByFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    {
        var list = convertPropertyToStrings(keySelector);
        return _this.ThenBy(list);
    }
    public static IQueryAndOrderByFluent<TEntity> ThenByDescending<TEntity, TProperty>(this IQueryAndOrderByFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> keySelector) where TEntity : class
    {
        var list = convertPropertyToStrings(keySelector);
        return _this.ThenByDescending(list);
    }

    public static IQueryAndIncludeFluent<TEntity, TProperty> Include<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, TProperty>> navigationPropertyPath) where TEntity : class where TProperty : class?
    {
        var list = convertPropertyToStrings(navigationPropertyPath);
        return _this.Include<TProperty>(list);
    }

    public static IQueryAndIncludeFluent<TEntity, TProperty> Include<TEntity, TProperty>(this IQueryFluent<TEntity> _this, Expression<Func<TEntity, ICollection<TProperty>>> navigationPropertyPath) where TEntity : class where TProperty : class?
    {
        var list = convertPropertyToStrings(navigationPropertyPath);
        return _this.Include<TProperty>(list);
    }

    public static IQueryAndIncludeFluent<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(this IQueryAndIncludeFluent<TEntity, TPreviousProperty> _this, Expression<Func<TPreviousProperty, TProperty?>> navigationPropertyPath) where TEntity : class where TPreviousProperty : class? where TProperty : class?
    {
        var list = convertPropertyToStrings(navigationPropertyPath);
        return _this.ThenInclude<TProperty>(list);
    }

    public static IQueryAndIncludeFluent<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty, TProperty>(this IQueryAndIncludeFluent<TEntity, ICollection<TPreviousProperty>> _this, Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath) where TEntity : class where TPreviousProperty : class? where TProperty : class?
    {
        var list = convertPropertyToStrings(navigationPropertyPath);
        return _this.ThenInclude<TProperty>(list);
    }

    #endregion

    #region ILtConnection

    public static int Count<TEntity>(this ILtConnection _this, IQueryFluent<TEntity> query) where TEntity : class
        => _this.Count(query.ToImmutable());
    public static int Count<TEntity, TParameter>(this ILtConnection _this, IQueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Count(query.ToImmutable(), values);

    public static IEnumerable<TEntity> Select<TEntity>(this ILtConnection _this, IQueryFluent<TEntity> query) where TEntity : class
        => _this.Select(query.ToImmutable());
    public static IEnumerable<TEntity> Select<TEntity, TParameter>(this ILtConnection _this, IQueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Select(query.ToImmutable(), values);

    public static TEntity Single<TEntity>(this ILtConnection _this, IQueryFluent<TEntity> query) where TEntity : class
        => _this.Single(query.ToImmutable());
    public static TEntity Single<TEntity, TParameter>(this ILtConnection _this, IQueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.Single(query.ToImmutable(), values);

    public static TEntity First<TEntity>(this ILtConnection _this, IQueryFluent<TEntity> query) where TEntity : class
        => _this.First(query.ToImmutable());
    public static TEntity First<TEntity, TParameter>(this ILtConnection _this, IQueryFluent<TEntity> query, TParameter values) where TEntity : class
        => _this.First(query.ToImmutable(), values);

    #endregion

    static List<string> convertPropertyToStrings<TEntity, TProperty>(Expression<Func<TEntity, TProperty>> exp) where TEntity : class?
    {
        var exp2 = (MemberExpression)exp.Body;

        var list = new List<string>();
        while (exp2 != null)
        {
            list.Add(exp2.Member.Name);
            exp2 = exp2.Expression as MemberExpression;
        }
        list.Reverse();
        return list;

    }


    static IValue convertToValue(Expression exp, IReadOnlyList<string>? parentProperty = default)
    {
        switch (exp)
        {
            case BinaryExpression binary:
                var lhs = convertToValue(binary.Left, parentProperty);
                var rhs = convertToValue(binary.Right, parentProperty);
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
                var list = convertToPropertyStrings(member, parentProperty);
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

                    return convertToValue(rhs2.Body, convertToPropertyStrings(lhs2, parentProperty));
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

    static List<string> convertToPropertyStrings(MemberExpression member, IReadOnlyList<string>? parentProperty)
    {
        var list = new List<string>();

        Expression? exp2 = member;
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
                    throw new ArgumentException("Argument must be PropertyAccess", nameof(member));
            }
        }
        if (parentProperty != null)
            list.AddRange(parentProperty);
        return list;
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
