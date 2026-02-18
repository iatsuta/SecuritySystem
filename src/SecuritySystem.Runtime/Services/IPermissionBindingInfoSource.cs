namespace SecuritySystem.Services;

public interface IPermissionBindingInfoSource
{
    PermissionBindingInfo GetForPermission(Type permissionType);

    PermissionBindingInfo<TPermission> GetForPermission<TPermission>() => (PermissionBindingInfo<TPermission>)this.GetForPermission(typeof(TPermission));

    PermissionBindingInfo GetForPrincipal(Type principalType);
}