using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using GenericQueryable;

using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystem<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    ISecurityRuleExpander securityRuleExpander,
    IUserNameResolver userNameResolver,
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential securityRuleCredential,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    : IPermissionSystem<TPermission>

    where TPermission : class
{
    public Type PermissionType { get; } = typeof(TPermission);

    public Expression<Func<TPermission, IEnumerable<TIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull =>
        bindingInfo.GetRestrictionsExpr(identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>(), restrictionFilterInfo?.GetPureFilter(serviceProvider));

    public Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext =>
        this.GetManyGrandAccessExpr<TSecurityContext>().BuildAnd();

    public Expression<Func<TPermission, bool>> GetContainsIdentsExpr<TSecurityContext, TIdent>(IEnumerable<TIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull =>
        this.GetManyContainsIdentsExpr(idents, restrictionFilterInfo).BuildOr();

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (securityRuleExpander.FullRoleExpand(securityRule).SecurityRoles.Contains(bindingInfo.SecurityRole))
        {
            return new VirtualPermissionSource<TPrincipal, TPermission>(
                serviceProvider,
                expressionEvaluatorStorage,
                identityInfoSource,
                userNameResolver,
                queryableSource,
                timeProvider,
                bindingInfo,
                securityRule,
                securityRuleCredential);
        }
        else
        {
            return new EmptyPermissionSource<TPermission>();
        }
    }

    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default) =>
        await this.GetPermissionSource(bindingInfo.SecurityRole).GetPermissionQuery().GenericAnyAsync(cancellationToken)
            ? [bindingInfo.SecurityRole]
            : [];

    private IEnumerable<Expression<Func<TPermission, bool>>> GetManyGrandAccessExpr<TSecurityContext>()
        where TSecurityContext : ISecurityContext
    {
        foreach (var restrictionPath in bindingInfo.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext?>> singlePath)
            {
                yield return singlePath.Select(securityContext => securityContext == null);
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                yield return manyPath.Select(securityContexts => !securityContexts.Any());
            }
        }
    }

    private IEnumerable<Expression<Func<TPermission, bool>>> GetManyContainsIdentsExpr<TSecurityContext, TIdent>(IEnumerable<TIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : ISecurityContext
        where TIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TIdent>();

        var filterExpr = identityInfo.CreateContainsFilter(idents.ToArray());

        foreach (var restrictionPath in bindingInfo.Restrictions)
        {
            if (restrictionPath is Expression<Func<TPermission, TSecurityContext>> singlePath)
            {
                if (restrictionFilterInfo == null)
                {
                    yield return singlePath.Select(filterExpr);
                }
                else
                {
                    var securityContextFilter = restrictionFilterInfo.GetPureFilter(serviceProvider)
                        .BuildAnd(filterExpr);

                    yield return singlePath.Select(securityContextFilter);
                }
            }
            else if (restrictionPath is Expression<Func<TPermission, IEnumerable<TSecurityContext>>> manyPath)
            {
                yield return ExpressionEvaluateHelper.InlineEvaluate(ee =>
                {
                    if (restrictionFilterInfo == null)
                    {
                        return manyPath.Select(securityContexts => securityContexts.Any(securityContext => ee.Evaluate(filterExpr, securityContext)));
                    }
                    else
                    {
                        var securityContextFilter = restrictionFilterInfo.GetPureFilter(serviceProvider).ToEnumerableAny()
                            .BuildAnd(securityContexts => securityContexts.Any(securityContext => ee.Evaluate(filterExpr, securityContext)));

                        return manyPath.Select(securityContextFilter);
                    }
                });
            }
        }
    }

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => this.GetPermissionSource(securityRule);
}