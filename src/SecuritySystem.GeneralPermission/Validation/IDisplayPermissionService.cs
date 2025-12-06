namespace SecuritySystem.GeneralPermission.Validation;

public interface IDisplayPermissionService<TPermission, TPermissionRestriction>
{
	string ToString(PermissionData<TPermission, TPermissionRestriction> permissionData);
}