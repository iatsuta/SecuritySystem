using CommonFramework.DependencyInjection;

using GenericQueryable;

using SecuritySystem.Expanders;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.VirtualPermission;

public class VirtualPermissionSystem<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    ISecurityRuleExpander securityRuleExpander,
    SecurityRuleCredential securityRuleCredential,
    VirtualPermissionBindingInfo<TPermission> virtualBindingInfo)
    : IPermissionSystem<TPermission>

    where TPermission : class
{
    public Type PermissionType { get; } = typeof(TPermission);

    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return serviceProxyFactory
            .Create<
                IPermissionRestrictionSource<TPermission, TSecurityContextIdent>,
                VirtualPermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>>(virtualBindingInfo, Tuple.Create(restrictionFilterInfo));
    }

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (securityRuleExpander.FullRoleExpand(securityRule).Children.SelectMany(c => c.SecurityRoles).Contains(virtualBindingInfo.SecurityRole))
        {
            return serviceProxyFactory.Create<IPermissionSource<TPermission>, VirtualPermissionSource<TPermission>>(virtualBindingInfo,
                securityRule with { CustomCredential = securityRuleCredential });
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