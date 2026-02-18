using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface IPermissionSecurityRoleIdentsFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule);
}