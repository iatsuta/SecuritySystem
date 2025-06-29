﻿using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;

namespace SecuritySystem.VirtualPermission;

public record VirtualPermissionBindingInfo<TPrincipal, TPermission>(
    SecurityRole SecurityRole,
    Expression<Func<TPermission, TPrincipal>> PrincipalPath,
    Expression<Func<TPrincipal, string>> PrincipalNamePath,
    IReadOnlyList<LambdaExpression> RestrictionPaths,
    Func<IServiceProvider, Expression<Func<TPermission, bool>>> GetFilter,
    Expression<Func<TPermission, Period>>? PeriodFilter = null)
{
    public VirtualPermissionBindingInfo(
        SecurityRole securityRole,
        Expression<Func<TPermission, TPrincipal>> principalPath,
        Expression<Func<TPrincipal, string>> principalNamePath)
        : this(securityRole, principalPath, principalNamePath, [], _ => _ => true)
    {
    }

    public Guid Id { get; } = Guid.NewGuid();

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext =>

        this with { RestrictionPaths = this.RestrictionPaths.Concat([path]).ToList() };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext =>

        this with { RestrictionPaths = this.RestrictionPaths.Concat([path]).ToList() };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
        Expression<Func<TPermission, bool>> filter) => this.AddFilter(_ => filter);

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
        Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter) =>

        this with { GetFilter = sp => this.GetFilter(sp).BuildAnd(getFilter(sp)) };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetPeriodFilter(
        Expression<Func<TPermission, Period>> periodFilter) =>
        this with { PeriodFilter = periodFilter };

    public IEnumerable<Type> GetSecurityContextTypes()
    {
        return this.RestrictionPaths
            .Select(restrictionPath => restrictionPath.ReturnType.GetCollectionElementTypeOrSelf())
            .Distinct();
    }

    public Expression<Func<TPermission, IEnumerable<Guid>>> GetRestrictionsExpr(Type securityContextType, LambdaExpression? pureFilter)
    {
        return new Func<Expression<Func<ISecurityContext, bool>>?, Expression<Func<TPermission, IEnumerable<Guid>>>>(this.GetRestrictionsExpr)
            .CreateGenericMethod(securityContextType)
            .Invoke<Expression<Func<TPermission, IEnumerable<Guid>>>>(this, pureFilter);
    }

    public Expression<Func<TPermission, IEnumerable<Guid>>> GetRestrictionsExpr<TSecurityContext>(Expression<Func<TSecurityContext, bool>>? pureFilter)
        where TSecurityContext : ISecurityContext
    {
        var expressions = this.GetManyRestrictionsExpr(pureFilter);

        return expressions.Match(
            () => _ => Array.Empty<Guid>(),
            single => single,
            many => many.Aggregate((state, expr) => from ids1 in state
                from ide2 in expr
                select ids1.Concat(ide2)));
    }

    private IEnumerable<Expression<Func<TPermission, IEnumerable<Guid>>>> GetManyRestrictionsExpr<TSecurityContext>(Expression<Func<TSecurityContext, bool>>? pureFilter)
        where TSecurityContext : ISecurityContext
    {
        foreach (var restrictionPath in this.RestrictionPaths)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                if (pureFilter == null)
                {
                    yield return singlePath.Select(securityContext => securityContext != null ? (IEnumerable<Guid>)new[] { securityContext.Id } : Array.Empty<Guid>());
                }
                else
                {
                    yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                        singlePath.Select(securityContext =>
                            securityContext != null && ee.Evaluate(pureFilter, securityContext) ? (IEnumerable<Guid>)new[] { securityContext.Id } : Array.Empty<Guid>()));
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                if (pureFilter == null)
                {
                    yield return manyPath.Select(securityContexts => securityContexts.Select(securityContext => securityContext.Id));
                }
                else
                {
                    yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                        manyPath.Select(securityContexts => securityContexts
                            .Where(securityContext => ee.Evaluate(pureFilter, securityContext))
                            .Select(securityContext => securityContext.Id)));
                }
            }
        }
    }
}