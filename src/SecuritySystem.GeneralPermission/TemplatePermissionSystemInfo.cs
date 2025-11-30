namespace SecuritySystem.GeneralPermission;

public abstract record GeneralPermissionSystemInfo<TPrincipal>
{
    public abstract Type PermissionType { get; }
}

public record GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    PropertyAccessors<TPermission, TPrincipal> ToPrincipal,
    PropertyAccessors<TPermission, TSecurityRole> ToSecurityRole,
    PropertyAccessors<TPermissionRestriction, TPermission> ToPermission,
    PropertyAccessors<TPermissionRestriction, TSecurityContextType> ToSecurityContextType,
    PropertyAccessors<TPermissionRestriction, TSecurityContextObjectIdent> ToSecurityContextObjectId)
    : GeneralPermissionSystemInfo<TPrincipal>, IPermissionToPrincipalInfo<TPrincipal, TPermission>

    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    public override Type PermissionType { get; } = typeof(TPermission);
}

public interface IPermissionToPrincipalInfo<TPrincipal, TPermission>
{
    PropertyAccessors<TPermission, TPrincipal> ToPrincipal { get; }
}