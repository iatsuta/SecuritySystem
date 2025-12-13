using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IAvailablePermissionFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}