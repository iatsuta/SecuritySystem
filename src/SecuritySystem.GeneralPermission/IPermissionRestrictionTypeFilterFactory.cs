using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext;
}