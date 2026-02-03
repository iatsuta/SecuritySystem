using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface IPermissionSecurityRoleFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> CreateFilter(Type identType, Array idents);
}