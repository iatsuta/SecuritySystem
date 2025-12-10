using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
	PropertyAccessors<TPermission, TPrincipal> Principal,
	PropertyAccessors<TPermission, TSecurityRole> SecurityRole,
	PropertyAccessors<TPermissionRestriction, TPermission> Permission,
	PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType,
	PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId)
	: GeneralPermissionSystemInfo,
		IPermissionToPrincipalInfo<TPermission, TPrincipal>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
	public PropertyAccessors<TPermission, string>? Comment { get; init; }

	public PropertyAccessors<TPermission, (DateTime StartDate, DateTime? EndDate)>? Period { get; init; }

	//public override Type PrincipalType { get; } = typeof(TPermission);
	//public abstract Type PermissionType { get; }
}

public abstract record GeneralPermissionSystemInfo
{
//	public abstract Type PrincipalType { get; }
//	public abstract Type PermissionType { get; }
//	public abstract Type PrincipalType { get; }
//	public abstract Type PrincipalType { get; }
//	public abstract Type PrincipalType { get; }
}