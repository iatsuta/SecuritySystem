using CommonFramework;

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

    public IEnumerable<IPermissionSource<TPermission>> GetPermissionSources(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        var expandedRoles = securityRuleExpander.FullRoleExpand(securityRule).Children.SelectMany(c => c.SecurityRoles).Distinct().ToHashSet();

        return

            from itemBindingInfo in virtualBindingInfo.Items

            where expandedRoles.Contains(itemBindingInfo.SecurityRole)

            select this.CreatePermissionSource(securityRule, itemBindingInfo);
    }

    public IAsyncEnumerable<SecurityRole> GetAvailableSecurityRoles() =>
        virtualBindingInfo
            .Items
            .ToAsyncEnumerable()
            .Where(async (itemBindingInfo, ct) =>
                await this.CreatePermissionSource(itemBindingInfo.SecurityRole, itemBindingInfo)
                    .GetPermissionQuery()
                    .GenericAnyAsync(ct))
            .Select(itemBindingInfo => itemBindingInfo.SecurityRole)
            .Distinct();

    private IPermissionSource<TPermission> CreatePermissionSource(
        DomainSecurityRule.RoleBaseSecurityRule securityRule,
        VirtualPermissionSecurityRoleItemBindingInfo<TPermission> itemBindingInfo)
    {
        return serviceProxyFactory.Create<IPermissionSource<TPermission>, VirtualPermissionSource<TPermission>>(
            virtualBindingInfo,
            itemBindingInfo,
            securityRule.TryApply(securityRuleCredential));
    }
}