namespace SecuritySystem.GeneralPermission;

public interface IGeneralPermissionRestrictionBindingInfoSource
{
    GeneralPermissionRestrictionBindingInfo GetForPermission(Type permissionType);

    GeneralPermissionRestrictionBindingInfo GetForPermissionRestriction(Type permissionRestrictionType);
}