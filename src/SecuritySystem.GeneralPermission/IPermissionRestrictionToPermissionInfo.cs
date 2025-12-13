using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionToPermissionInfo<TPermissionRestriction, TPermission>
{
	PropertyAccessors<TPermissionRestriction, TPermission> Permission { get; }
}