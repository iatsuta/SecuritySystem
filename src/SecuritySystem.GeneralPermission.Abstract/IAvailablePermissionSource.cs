namespace SecuritySystem.GeneralPermission;

public interface IAvailablePermissionSource<out TPermission, TSecurityContextObjectIdent>
{
    AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter<TSecurityContextObjectIdent> filter);
}