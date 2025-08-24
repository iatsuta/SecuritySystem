using CommonFramework;
using SecuritySystem.ExpressionEvaluate;
using System.Linq.Expressions;

namespace SecuritySystem.VirtualPermission;

public record VirtualPermissionBindingInfo<TPrincipal, TPermission>(
    SecurityRole SecurityRole,
    Expression<Func<TPermission, TPrincipal>> PrincipalPath,
    Expression<Func<TPrincipal, string>> PrincipalNamePath,
    IReadOnlyList<LambdaExpression> RestrictionPaths,
    Func<IServiceProvider, Expression<Func<TPermission, bool>>> GetFilter,
    Expression<Func<TPermission, DateTime>>? StartDateFilter = null,
    Expression<Func<TPermission, DateTime?>>? EndDateFilter = null)
{
    public VirtualPermissionBindingInfo(
        SecurityRole securityRole,
        Expression<Func<TPermission, TPrincipal>> principalPath,
        Expression<Func<TPrincipal, string>> principalNamePath)
        : this(securityRole, principalPath, principalNamePath, [], _ => _ => true)
    {
    }

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

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetStartDateFilter(
        Expression<Func<TPermission, DateTime>> startDateFilter) =>
        this with { StartDateFilter = startDateFilter };

    public VirtualPermissionBindingInfo<TPrincipal, TPermission> SetEndDateFilter(
        Expression<Func<TPermission, DateTime?>> endDateFilter) =>
        this with { EndDateFilter = endDateFilter };

    public IEnumerable<Type> GetSecurityContextTypes()
    {
        return this.RestrictionPaths
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
        return this.RestrictionPaths.Select(restrictionPath =>
            {
                return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                {
                    if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
                    {
                        if (pureFilter == null)
                        {
                            return singlePath.Select(IEnumerable<TIdent> (securityContext) =>
                                securityContext != null ? new[] { ee.Evaluate(identityInfo.IdPath, securityContext) } : Array.Empty<TIdent>());
                        }
                        else
                        {
                            return singlePath.Select(IEnumerable<TIdent> (securityContext) =>
                                securityContext != null && ee.Evaluate(pureFilter, securityContext)
                                    ? new[] { ee.Evaluate(identityInfo.IdPath, securityContext) }
                                    : Array.Empty<TIdent>());
                        }
                    }
                    else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
                    {
                        if (pureFilter == null)
                        {
                            return manyPath.Select(securityContexts => securityContexts.Select(securityContext => ee.Evaluate(identityInfo.IdPath, securityContext)));
                        }
                        else
                        {
                            return manyPath.Select(securityContexts => securityContexts
                                .Where(securityContext => ee.Evaluate(pureFilter, securityContext))
                                .Select(securityContext => ee.Evaluate(identityInfo.IdPath, securityContext)));
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                });
            }
        );
    }
}