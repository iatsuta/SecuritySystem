using CommonFramework.IdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystem<TPrincipal, TPermission>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    ISecurityRuleExpander securityRuleExpander,
    SecurityRuleCredential securityRuleCredential,
    VirtualPermissionBindingInfo<TPrincipal, TPermission> bindingInfo)
    : IPermissionSystem<TPermission>

    where TPermission : class
{
    public Type PermissionType { get; } = typeof(TPermission);


    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return new VirtualPermissionRestrictionSource<TPrincipal, TPermission, TSecurityContext, TSecurityContextIdent>(serviceProvider, identityInfoSource,
            bindingInfo, restrictionFilterInfo);
    }

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


    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => this.GetPermissionSource(securityRule);
}