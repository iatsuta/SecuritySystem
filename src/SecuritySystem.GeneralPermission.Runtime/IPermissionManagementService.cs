using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionManagementService<in TPrincipal, TPermission, TPermissionRestriction>
{
    Task<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken);

    Task<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken);

    Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(
        TPermission dbPermission,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken);
}