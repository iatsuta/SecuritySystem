using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface IAvailablePermissionFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}