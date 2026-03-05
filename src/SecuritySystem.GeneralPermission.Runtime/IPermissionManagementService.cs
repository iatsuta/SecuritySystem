using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionManagementService<in TPrincipal, TPermission, TPermissionRestriction>
{
    ValueTask<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken);

    ValueTask<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken);

    ValueTask<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(
        TPermission dbPermission,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken);
}