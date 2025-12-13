namespace SecuritySystem.GeneralPermission.Validation;

public interface IPermissionSecurityRoleResolver<in TPermission>
{
	FullSecurityRole ResolveRole(TPermission permission);
}