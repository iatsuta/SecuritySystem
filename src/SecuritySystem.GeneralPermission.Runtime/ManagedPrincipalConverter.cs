using CommonFramework;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(
    IServiceProxyFactory serviceProxyFactory,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var innerServiceType = typeof(ManagedPrincipalConverter<,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            restrictionBindingInfo.PermissionRestrictionType);

        return serviceProxyFactory.Create<IManagedPrincipalConverter<TPrincipal>>(innerServiceType);
    });

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission, TPermissionRestriction>(
    IManagedPrincipalHeaderConverter<TPrincipal> headerConverter,
    IPermissionLoader<TPrincipal, TPermission> permissionLoader,
    IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction> permissionManagementService) : IManagedPrincipalConverter<TPrincipal>
    where TPrincipal : class
    where TPermission : class
{
    public async Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal dbPrincipal, CancellationToken cancellationToken)
    {
        var dbPermissions = await permissionLoader.LoadAsync(dbPrincipal, cancellationToken);

        var permissions = await dbPermissions.SyncWhenAll(permission => permissionManagementService.ToManagedPermissionAsync(permission, cancellationToken));

        return new ManagedPrincipal(headerConverter.Convert(dbPrincipal), [..permissions]);
    }
}