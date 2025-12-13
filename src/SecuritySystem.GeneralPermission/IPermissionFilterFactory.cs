using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public interface IPermissionFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> GetSecurityContextFilter<TSecurityContext>(SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext;
}