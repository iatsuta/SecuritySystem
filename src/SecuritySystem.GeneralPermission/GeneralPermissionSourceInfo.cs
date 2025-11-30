using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionSourceInfo<TPrincipal, TPermission, TPermissionRestriction, TSecurityContextType, TSecurityRole>(
	Expression<Func<TPrincipal, string>> PrincipalNamePath,
	Expression<Func<TPermission, TPrincipal>> PrincipalPath,
	Expression<Func<TPermission, IEnumerable<TPermissionRestriction>>> RestrictionPath,
	Expression<Func<TPermissionRestriction, TSecurityContextType>> SecurityContextTypePath,
	Expression<Func<TPermission, TSecurityRole>> SecurityRolePath) : GeneralPermissionSystemInfo<TPrincipal>;

public abstract record GeneralPermissionSystemInfo<TPrincipal>
{
	public abstract Type PermissionType { get; }
}