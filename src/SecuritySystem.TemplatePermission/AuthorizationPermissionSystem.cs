using System.Linq.Expressions;
using CommonFramework;
using Framework.Authorization.Domain;

using Microsoft.Extensions.DependencyInjection;
using SecuritySystem;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationPermissionSystem(
    IServiceProvider serviceProvider,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential securityRuleCredential)
    : IPermissionSystem<TPermission>
{
    public Type PermissionType { get; } = typeof(TPermission);

    public Expression<Func<TPermission, IEnumerable<TIdent>>> GetPermissionRestrictionsExpr<TSecurityContext, TIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TIdent : notnull
    {
        if (typeof(TIdent) != typeof(Guid))
        {
            throw new InvalidOperationException($"{nameof(TIdent)} must be {nameof(Guid)}");
        }
        else
        {
            return (Expression<Func<TPermission, IEnumerable<TIdent>>>)(object)this.GetPermissionRestrictionsExpr(restrictionFilterInfo);
        }
    }

    private Expression<Func<TPermission, IEnumerable<Guid>>> GetPermissionRestrictionsExpr<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var securityContextTypeId = securityContextInfoSource.GetSecurityContextInfo<TSecurityContext>().Id;

        if (restrictionFilterInfo == null)
        {
            return permission => permission.Restrictions
                                           .Where(restriction => restriction.SecurityContextType.Id == securityContextTypeId)
                                           .Select(restriction => restriction.SecurityContextId);
        }
        else
        {
            var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, Guid>();

            var securityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                                                                .Where(restrictionFilterInfo.GetPureFilter(serviceProvider))
                                                                .Select(identityInfo.IdPath);

            return permission => permission.Restrictions
                                           .Where(restriction => restriction.SecurityContextType.Id == securityContextTypeId)
                                           .Where(restriction => securityContextQueryable.Contains(restriction.SecurityContextId))
                                           .Select(restriction => restriction.SecurityContextId);
        }
    }

    public Expression<Func<TPermission, bool>> GetGrandAccessExpr<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext =>

        this.GetPermissionRestrictionsExpr<TSecurityContext>(null).Select(v => !v.Any());

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return ActivatorUtilities.CreateInstance<AuthorizationPermissionSource>(
            serviceProvider,
            securityRule.TryApplyCredential(securityRuleCredential));
    }

    public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default)
    {
        return ActivatorUtilities.CreateInstance<AuthorizationAvailableSecurityRoleSource>(serviceProvider, securityRuleCredential)
                                 .GetAvailableSecurityRoles(cancellationToken);
    }

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return this.GetPermissionSource(securityRule);
    }
}
