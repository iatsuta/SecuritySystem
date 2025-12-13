using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>(SecurityContextRestriction<TSecurityContext>? restriction)
        where TSecurityContext : class, ISecurityContext;
}