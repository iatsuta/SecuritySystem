using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IDisplayPermissionService<TPermission, TPermissionRestriction>
{
    string Format(PermissionData<TPermission, TPermissionRestriction> permissionData);
}