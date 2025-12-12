using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext;
}