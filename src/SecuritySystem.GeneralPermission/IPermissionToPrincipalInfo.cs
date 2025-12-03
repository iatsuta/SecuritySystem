using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionToPrincipalInfo<TPrincipal, TPermission>
{
	PropertyAccessors<TPermission, TPrincipal> ToPrincipal { get; }
}

public interface IPermissionToPrincipalNameInfo<TPermission>
{
	Expression<Func<TPermission, string>> ToPrincipalNamePath { get; }
}