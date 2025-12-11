using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
	PropertyAccessors<TPermission, TPrincipal> Principal,
	PropertyAccessors<TPermission, TSecurityRole> SecurityRole,
	PropertyAccessors<TPermissionRestriction, TPermission> Permission,
	PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType,
	PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId)
	: GeneralPermissionBindingInfo<TPrincipal, TPermission>,
		IPermissionToPrincipalInfo<TPermission, TPrincipal>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
    public override Type SecurityRoleType { get; } = typeof(TSecurityRole);

    public override Type PermissionRestrictionType { get; } = typeof(TPermissionRestriction);

    public override Type SecurityContextTypeType { get; } = typeof(TSecurityContextType);

    public override Type SecurityContextObjectIdentType { get; } = typeof(TSecurityContextObjectIdent);
}
public abstract record GeneralPermissionBindingInfo<TPrincipal, TPermission> : GeneralPermissionBindingInfo
{
    public override Type PrincipalType { get; } = typeof(TPrincipal);

    public override Type PermissionType { get; } = typeof(TPermission);

    public PropertyAccessors<TPermission, string>? Comment { get; init; }

    public PropertyAccessors<TPermission, (DateTime StartDate, DateTime? EndDate)>? Period { get; init; }
}

public abstract record GeneralPermissionBindingInfo
{
    public abstract Type PrincipalType { get; }

    public abstract Type PermissionType { get; }

    public abstract Type SecurityRoleType { get; }

    public abstract Type PermissionRestrictionType { get; }

    public abstract Type SecurityContextTypeType { get; }

    public abstract Type SecurityContextObjectIdentType { get; }
}