using System.Linq.Expressions;
using CommonFramework.GenericRepository;

namespace SecuritySystem.GeneralPermission;

public class PermissionFilterFactory<TPermission> : IPermissionFilterFactory<TPermission>
{
    public Expression<Func<TPermission, bool>> GetSecurityContextFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        throw new NotImplementedException();
    }
}

public class PermissionFilterFactory<TPermission, TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory
) : IPermissionFilterFactory<TPermission>
    where TPermissionRestriction : class
{
    public Expression<Func<TPermission, bool>> GetSecurityContextFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        var typeFilter = permissionRestrictionFilterFactory.GetFilter<TSecurityContext>();

        var restrictionFilter = securityContextRestriction.Filter == null ? _ => true : securityContextRestriction.Filter.GetPureFilter(serviceProvider);

        var requiredFilter = securityContextRestriction.Required ? queryableSource.GetQueryable<TPermissionRestriction>().an

        throw new NotImplementedException();
    }

    private Expression<Func<TPermissionRestriction, bool>> GetRestrictionFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {

    }
}