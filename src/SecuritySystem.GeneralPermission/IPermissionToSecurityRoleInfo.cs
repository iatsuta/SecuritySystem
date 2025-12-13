using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionToSecurityRoleInfo<TPermission, TSecurityRole>
{
	PropertyAccessors<TPermission, TSecurityRole> SecurityRole { get; }
}