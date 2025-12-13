using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> GetFilter(SecurityContextRestrictionFilterInfo? restrictionFilterInfo);

    Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>(SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext;
}