namespace SecuritySystem.GeneralPermission;

public interface IAvailablePermissionSource<out TPermission>
{
    IQueryable<TPermission> GetQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}