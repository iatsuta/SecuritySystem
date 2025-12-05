namespace SecuritySystem.GeneralPermission;

public interface IPermissionToSecurityRoleInfo<TPermission, TSecurityRole>
{
	PropertyAccessors<TPermission, TSecurityRole> ToPrincipal { get; }
}