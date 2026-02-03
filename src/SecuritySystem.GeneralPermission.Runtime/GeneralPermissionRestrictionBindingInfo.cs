using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission>

    : GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
{
    public required PropertyAccessors<TPermissionRestriction, TPermission> Permission { get; init; }


    public sealed override Type PermissionType { get; } = typeof(TPermission);
}

public abstract record GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
    : GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType>
{
    public required PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId { get; init; }

    public sealed override Type SecurityContextObjectIdentType { get; } = typeof(TSecurityContextObjectIdent);
}

public abstract record GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType>
    : GeneralPermissionRestrictionBindingInfo
{
    public required PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType { get; init; }


    public sealed override Type PermissionRestrictionType { get; } = typeof(TPermissionRestriction);

    public sealed override Type SecurityContextTypeType { get; } = typeof(TSecurityContextType);
}

public abstract record GeneralPermissionRestrictionBindingInfo
{
    public abstract Type PermissionType { get; }

    public abstract Type PermissionRestrictionType { get; }

    public abstract Type SecurityContextTypeType { get; }

    public abstract Type SecurityContextObjectIdentType { get; }
}