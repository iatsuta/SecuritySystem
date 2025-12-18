using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission.Validation;

public interface IDisplayPermissionService<in TPermissionData>
    where TPermissionData : PermissionData
{
    string ToString(TPermissionData permissionData);
}