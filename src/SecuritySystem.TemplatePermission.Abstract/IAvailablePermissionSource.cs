namespace SecuritySystem.TemplatePermission;

public interface IAvailablePermissionSource<out TPermission>
{
    AvailablePermissionFilter CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);

    IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter filter);
}
a