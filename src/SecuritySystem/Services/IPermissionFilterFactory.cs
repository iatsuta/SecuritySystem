using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface IPermissionFilterFactory<TPermission>
{
    Expression<Func<TPermission, bool>> CreateFilter(SecurityContextRestriction securityContextRestriction);

    Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>(SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext;
}