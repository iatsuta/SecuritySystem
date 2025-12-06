using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionToPrincipalInfo<TPermission, TPrincipal>
{
	PropertyAccessors<TPermission, TPrincipal> Principal { get; }
}