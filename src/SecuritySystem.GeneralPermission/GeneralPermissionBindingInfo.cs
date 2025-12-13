using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>

	: GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole>,
		IPermissionToPrincipalInfo<TPermission, TPrincipal>,
        IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>,
        IPermissionRestrictionToPermissionInfo<TPermissionRestriction, TPermission>
{
    public required PropertyAccessors<TPermissionRestriction, TPermission> Permission { get; init; }

    public required PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType { get; init; }

    public required PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId { get; init; }


    public override Type PermissionRestrictionType { get; } = typeof(TPermissionRestriction);

    public override Type SecurityContextTypeType { get; } = typeof(TSecurityContextType);

    public override Type SecurityContextObjectIdentType { get; } = typeof(TSecurityContextObjectIdent);
}

public abstract record GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole> : GeneralPermissionBindingInfo<TPrincipal, TPermission>
{
    public required PropertyAccessors<TPermission, TSecurityRole> SecurityRole { get; init; }

    public override Type SecurityRoleType { get; } = typeof(TSecurityRole);
}

public abstract record GeneralPermissionBindingInfo<TPrincipal, TPermission> : GeneralPermissionBindingInfo
{
    public required PropertyAccessors<TPermission, TPrincipal> Principal { get; init; }


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

public interface IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>
{
    PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType { get; }
}

public interface
    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> :
    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>
{
    PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId { get; }
}