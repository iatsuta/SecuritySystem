namespace SecuritySystem.GeneralPermission;

public interface IPermissionToPrincipalInfo<TPrincipal, TPermission>
{
	PropertyAccessors<TPermission, TPrincipal> ToPrincipal { get; }
}