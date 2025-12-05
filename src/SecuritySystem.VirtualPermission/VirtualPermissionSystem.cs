using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using GenericQueryable;

using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;
using System.Linq.Expressions;
using CommonFramework.IdentitySource;
using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystem<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    ISecurityRuleExpander securityRuleExpander,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential securityRuleCredential,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    : IPermissionSystem<TPermission>

    where TPermission : class
{
    public Type PermissionType { get; } = typeof(TPermission);

    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull =>
        bindingInfo.GetRestrictionsExpr(identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>(), restrictionFilterInfo?.GetPureFilter(serviceProvider));

    public Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext, TSecurityContextIdent>()
        where TSecurityContext : class, ISecurityContext
		where TSecurityContextIdent : notnull =>
        this.GetManyGrandAccessExpr<TSecurityContext>().BuildAnd();

    public Expression<Func<TPermission, bool>> GetContainsIdentsExpr<TSecurityContext, TSecurityContextIdent>(IEnumerable<TSecurityContextIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull =>
        this.GetManyContainsIdentsExpr(idents, restrictionFilterInfo).BuildOr();

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (securityRuleExpander.FullRoleExpand(securityRule).SecurityRoles.Contains(bindingInfo.SecurityRole))
        {
            return ActivatorUtilities.CreateInstance<VirtualPermissionSource<TPrincipal, TPermission>>(
                serviceProvider,
                securityRuleCredential,
				bindingInfo,
				securityRule);
        }
        else
        {
            return new EmptyPermissionSource<TPermission>();
        }
    }

    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken) =>
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

    private IEnumerable<Expression<Func<TPermission, bool>>> GetManyContainsIdentsExpr<TSecurityContext, TSecurityContextIdent>(IEnumerable<TSecurityContextIdent> idents,
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : ISecurityContext
        where TSecurityContextIdent : notnull
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

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