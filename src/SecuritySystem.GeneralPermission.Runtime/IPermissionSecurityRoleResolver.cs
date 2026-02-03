namespace SecuritySystem.GeneralPermission;

public interface IPermissionSecurityRoleResolver<in TPermission>
{
    FullSecurityRole Resolve(TPermission permission);
}