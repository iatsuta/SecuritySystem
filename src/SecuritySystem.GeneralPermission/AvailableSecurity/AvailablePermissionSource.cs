using CommonFramework.GenericRepository;

namespace SecuritySystem.GeneralPermission.AvailableSecurity;

public class AvailablePermissionSource<TPermission>(
    IQueryableSource queryableSource,
    IAvailablePermissionFilterFactory<TPermission> availablePermissionFilterFactory) : IAvailablePermissionSource<TPermission>
    where TPermission : class
{
    public IQueryable<TPermission> GetQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return queryableSource.GetQueryable<TPermission>().Where(availablePermissionFilterFactory.CreateFilter(securityRule));
    }
}