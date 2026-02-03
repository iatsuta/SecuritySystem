namespace SecuritySystem.GeneralPermission;

public interface IGeneralPermissionBindingInfoSource
{
    GeneralPermissionBindingInfo GetForPermission(Type permissionType);

    GeneralPermissionBindingInfo GetForSecurityRole(Type securityRoleType);
}