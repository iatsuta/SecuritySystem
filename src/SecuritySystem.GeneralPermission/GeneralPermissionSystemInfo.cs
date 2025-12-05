using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
	PropertyAccessors<TPermission, TPrincipal> ToPrincipal,
	PropertyAccessors<TPermission, TSecurityRole> ToSecurityRole,
	PropertyAccessors<TPermissionRestriction, TPermission> ToPermission,
	PropertyAccessors<TPermissionRestriction, TSecurityContextType> ToSecurityContextType,
	PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> ToSecurityContextObjectId,
	PropertyAccessors<TPermission, string>? Comment,
	PropertyAccessors<TPermission, DateTime>? StartDate,
	PropertyAccessors<TPermission, DateTime?>? EndDate)
	: GeneralPermissionSystemInfo<TPrincipal>,
		IPermissionToPrincipalInfo<TPermission, TPrincipal>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
	public override Type PermissionType { get; } = typeof(TPermission);
}

public abstract record GeneralPermissionSystemInfo<TPrincipal>
{
	public abstract Type PermissionType { get; }
}