namespace SecuritySystem.Services;

public interface IPermissionBindingInfoSource
{
    PermissionBindingInfo GetForPermission(Type permissionType);

    PermissionBindingInfo GetForPrincipal(Type principalType);
}