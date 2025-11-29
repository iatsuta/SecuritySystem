using CommonFramework;

using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;

namespace SecuritySystem.VirtualPermission;

public record VirtualPermissionBindingInfo<TPrincipal, TPermission>(
    SecurityRole SecurityRole,
    Expression<Func<TPermission, TPrincipal>> PrincipalPath,
    Expression<Func<TPrincipal, string>> PrincipalNamePath)
{
    public IReadOnlyList<LambdaExpression> Restrictions { get; init; } = [];

    public Func<IServiceProvider, Expression<Func<TPermission, bool>>> GetFilter { get; init; } = _ => _ => true;

    public Expression<Func<TPermission, DateTime>>? StartDateFilter { get; init; }

    public Expression<Func<TPermission, DateTime?>>? EndDateFilter { get; init; }


	public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, IEnumerable<TSecurityContext>>> path)
        where TSecurityContext : ISecurityContext =>

        this with { Restrictions = this.Restrictions.Concat([path]).ToList() };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddRestriction<TSecurityContext>(
        Expression<Func<TPermission, TSecurityContext?>> path)
        where TSecurityContext : ISecurityContext =>

        this with { Restrictions = this.Restrictions.Concat([path]).ToList() };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
        Expression<Func<TPermission, bool>> filter) => this.AddFilter(_ => filter);

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> AddFilter(
        Func<IServiceProvider, Expression<Func<TPermission, bool>>> getFilter) =>

        this with { GetFilter = sp => this.GetFilter(sp).BuildAnd(getFilter(sp)) };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetStartDateFilter(
        Expression<Func<TPermission, DateTime>> startDateFilter) =>
        this with { StartDateFilter = startDateFilter };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetEndDateFilter(
        Expression<Func<TPermission, DateTime?>> endDateFilter) =>
        this with { EndDateFilter = endDateFilter };

    public IEnumerable<Type> GetSecurityContextTypes()
    {
        return this.Restrictions
			.Select(restrictionPath => restrictionPath.ReturnType.GetCollectionElementTypeOrSelf())
            .Distinct();
    }

    public Expression<Func<TPermission, Array>> GetRestrictionsArrayExpr(IdentityInfo identityInfo, LambdaExpression? pureFilter)
    {
        return new Func<IdentityInfo<ISecurityContext, Ignore>, Expression<Func<ISecurityContext, bool>>?, Expression<Func<TPermission, Array>>>(
                this.GetRestrictionsArrayExpr)
            .CreateGenericMethod(identityInfo.DomainObjectType, identityInfo.IdentityType)
            .Invoke<Expression<Func<TPermission, Array>>>(this, identityInfo, pureFilter);
    }

    public Expression<Func<TPermission, Array>> GetRestrictionsArrayExpr<TSecurityContext, TIdent>(IdentityInfo<TSecurityContext, TIdent> identityInfo,
        Expression<Func<TSecurityContext, bool>>? pureFilter)
        where TSecurityContext : ISecurityContext where TIdent : notnull
    {
        return from idents in this.GetRestrictionsExpr(identityInfo, pureFilter)

            select (Array)idents.ToArray();
    }

    public Expression<Func<TPermission, IEnumerable<TIdent>>> GetRestrictionsExpr<TSecurityContext, TIdent>(IdentityInfo<TSecurityContext, TIdent> identityInfo,
        Expression<Func<TSecurityContext, bool>>? pureFilter)
        where TSecurityContext : ISecurityContext where TIdent : notnull
    {
        var expressions = this.GetManyRestrictionsExpr(identityInfo, pureFilter);

        return expressions.Match(
            () => _ => Array.Empty<TIdent>(),
            single => single,
            many => many.Aggregate((state, expr) =>
                from ids1 in state
                from ide2 in expr
                select ids1.Concat(ide2)));
    }

    private IEnumerable<Expression<Func<TPermission, IEnumerable<TIdent>>>> GetManyRestrictionsExpr<TSecurityContext, TIdent>(
        IdentityInfo<TSecurityContext, TIdent> identityInfo, Expression<Func<TSecurityContext, bool>>? pureFilter)
        where TSecurityContext : ISecurityContext where TIdent : notnull
    {
        foreach (var restrictionPath in this.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                {
                    if (pureFilter == null)
                    {
                        return singlePath.Select(IEnumerable<TIdent> (securityContext) =>
                            securityContext != null ? new[] { ee.Evaluate(identityInfo.Id.Path, securityContext) } : Array.Empty<TIdent>());
                    }
                    else
                    {
                        return singlePath.Select(IEnumerable<TIdent> (securityContext) =>
                            securityContext != null && ee.Evaluate(pureFilter, securityContext)
                                ? new[] { ee.Evaluate(identityInfo.Id.Path, securityContext) }
                                : Array.Empty<TIdent>());
                    }
                });
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                {
                    if (pureFilter == null)
                    {
                        return manyPath.Select(securityContexts => securityContexts.Select(securityContext => ee.Evaluate(identityInfo.Id.Path, securityContext)));
                    }
                    else
                    {
                        return manyPath.Select(securityContexts => securityContexts
                            .Where(securityContext => ee.Evaluate(pureFilter, securityContext))
                            .Select(securityContext => ee.Evaluate(identityInfo.Id.Path, securityContext)));
                    }
                });
            }
        }
    }
}