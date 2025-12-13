using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext;
}