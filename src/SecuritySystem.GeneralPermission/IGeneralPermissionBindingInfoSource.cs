namespace SecuritySystem.GeneralPermission;

public interface IGeneralPermissionBindingInfoSource
{
    GeneralPermissionBindingInfo GetForPrincipal(Type principalType);

    GeneralPermissionBindingInfo GetForPermission(Type permissionType);

    GeneralPermissionBindingInfo GetForSecurityRole(Type securityRoleType);
}