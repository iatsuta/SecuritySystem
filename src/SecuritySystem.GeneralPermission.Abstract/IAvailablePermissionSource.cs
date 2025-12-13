namespace SecuritySystem.GeneralPermission;

public interface IAvailablePermissionSource<out TPermission>
{
    IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}