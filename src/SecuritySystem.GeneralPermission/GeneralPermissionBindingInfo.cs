using CommonFramework;

namespace SecuritySystem.GeneralPermission;

public record GeneralPermissionBindingInfo<TPermission, TPrincipal, TSecurityRole> : GeneralPermissionBindingInfo<TPermission, TPrincipal>
{
    public sealed override Type SecurityRoleType { get; } = typeof(TSecurityRole);

    public required PropertyAccessors<TPermission, TSecurityRole> SecurityRole { get; init; }

    public PropertyAccessors<TSecurityRole, string>? SecurityRoleDescription { get; init; }
}

public abstract record GeneralPermissionBindingInfo<TPermission, TPrincipal> : GeneralPermissionBindingInfo<TPermission>
{
    public sealed override Type PrincipalType { get; } = typeof(TPrincipal);

    public required PropertyAccessors<TPermission, TPrincipal> Principal { get; init; }
}

public abstract record GeneralPermissionBindingInfo<TPermission> : GeneralPermissionBindingInfo
{
    public sealed override Type PermissionType { get; } = typeof(TPermission);

    public PropertyAccessors<TPermission, string>? PermissionComment { get; init; }

    public PropertyAccessors<TPermission, (DateTime StartDate, DateTime? EndDate)>? PermissionPeriod { get; init; }
}

public abstract record GeneralPermissionBindingInfo
{
    public bool IsReadonly { get; init; }

    public abstract Type PrincipalType { get; }

    public abstract Type PermissionType { get; }

    public abstract Type SecurityRoleType { get; }
}

//public interface IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>
//{
//    PropertyAccessors<TPermissionRestriction, TSecurityContextType> SecurityContextType { get; }
//}

//public interface
//    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> :
//    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>
//{
//    PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> SecurityContextObjectId { get; }
//}

//.AddSingleton<GeneralPermissionBindingInfo>(finalBindingInfo)
//.AddSingletonFrom<GeneralPermissionBindingInfo<TPrincipal, TPermission>, GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole
//    , TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
//.AddSingletonFrom<GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole>, GeneralPermissionBindingInfo<TPrincipal, TPermission
//    , TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
//.AddSingletonFrom<IPermissionRestrictionToPermissionInfo<TPermissionRestriction, TPermission>, GeneralPermissionBindingInfo<TPrincipal,
//    TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
//.AddSingletonFrom<IPermissionToPrincipalInfo<TPermission, TPrincipal>, GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole,
//    TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>>()
//.AddSingletonFrom<IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
//    , GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
//        TSecurityContextObjectIdent>>()
//.AddSingletonFrom<IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType>,
//    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
//        TSecurityContextObjectIdent>>()