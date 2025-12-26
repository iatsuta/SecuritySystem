using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext;
}