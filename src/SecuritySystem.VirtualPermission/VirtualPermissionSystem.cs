using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystem<TPermission>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    ISecurityRuleExpander securityRuleExpander,
    SecurityRuleCredential securityRuleCredential,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo,
    IPermissionBindingInfoSource bindingInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource)
    : IPermissionSystem<TPermission>

    where TPermission : class
{
    public Type PermissionType { get; } = typeof(TPermission);

    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return new VirtualPermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>(serviceProvider, identityInfoSource,
            virtualBindingInfo, restrictionFilterInfo);
    }

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (securityRuleExpander.FullRoleExpand(securityRule).SecurityRoles.Contains(virtualBindingInfo.SecurityRole))
        {
            return new VirtualPermissionSource<TPermission>(serviceProvider, visualIdentityInfoSource, bindingInfoSource, virtualBindingInfo, securityRule,
                securityRuleCredential);
        }
        else
        {
            return new EmptyPermissionSource<TPermission>();
        }
    }

    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken) =>
        await this.GetPermissionSource(virtualBindingInfo.SecurityRole).GetPermissionQuery().GenericAnyAsync(cancellationToken)
            ? [virtualBindingInfo.SecurityRole]
            : [];


    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => this.GetPermissionSource(securityRule);
}