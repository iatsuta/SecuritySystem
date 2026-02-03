using CommonFramework;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class PermissionSecurityRoleResolver<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource) : IPermissionSecurityRoleResolver<TPermission>
{
    private readonly Lazy<IPermissionSecurityRoleResolver<TPermission>> lazyInnerService = new(() =>
    {
        var generalBindingInfo = generalBindingInfoSource.GetForPermission(typeof(TPermission));

        var innerServiceType = typeof(PermissionSecurityRoleResolver<,>)
            .MakeGenericType(generalBindingInfo.PermissionType, generalBindingInfo.SecurityRoleType);

        return serviceProxyFactory.Create<IPermissionSecurityRoleResolver<TPermission>>(
            innerServiceType,
            generalBindingInfo);
    });

    public FullSecurityRole Resolve(TPermission permission) => this.lazyInnerService.Value.Resolve(permission);
}

public class PermissionSecurityRoleResolver<TPermission, TSecurityRole>(
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    ISecurityIdentityExtractor<TSecurityRole> securityRoleIdentityExtractor,
    ISecurityRoleSource securityRoleSource) : IPermissionSecurityRoleResolver<TPermission>
{
    public FullSecurityRole Resolve(TPermission permission)
    {
        var dbSecurityRole = generalBindingInfo.SecurityRole.Getter(permission);

        return securityRoleSource.GetSecurityRole(securityRoleIdentityExtractor.Extract(dbSecurityRole));
    }
}