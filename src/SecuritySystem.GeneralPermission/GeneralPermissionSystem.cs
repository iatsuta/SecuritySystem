using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.GeneralPermission.AvailableSecurityRoleSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystem<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    SecurityRuleCredential securityRuleCredential)
    : IPermissionSystem<TPermission>

    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    public Type PermissionType { get; } = typeof(TPermission);

    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return ActivatorUtilities
            .CreateInstance<GeneralPermissionRestrictionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                TSecurityContextObjectIdent, TSecurityContext, TSecurityContextIdent>>(serviceProvider, identityInfoSource, bindingInfo,
                new Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?>(restrictionFilterInfo));
    }

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return ActivatorUtilities
            .CreateInstance<GeneralPermissionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
                TSecurityContextObjectIdent>>(
                serviceProvider,
                securityRule.TryApplyCredential(securityRuleCredential));
    }

    public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
    {
        return ActivatorUtilities
            .CreateInstance<GeneralAvailableSecurityRoleSource<TPrincipal, TPermission, TSecurityRole>>(serviceProvider, securityRuleCredential)
            .GetAvailableSecurityRoles(cancellationToken);
    }

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return this.GetPermissionSource(securityRule);
    }
}