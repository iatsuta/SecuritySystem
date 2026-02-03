using CommonFramework;
using CommonFramework.GenericRepository;

using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionFilterFactory<TPermission>(IServiceProxyFactory serviceProxyFactory, IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPermissionFilterFactory<TPermission>
{
    private readonly Lazy<IPermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(typeof(TPermission));

        var innerServiceType =
            typeof(PermissionFilterFactory<,,,>).MakeGenericType(
                restrictionBindingInfo.PermissionType,
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionFilterFactory<TPermission>>(innerServiceType, restrictionBindingInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(SecurityContextRestriction securityContextRestriction) =>
        this.lazyInnerService.Value.CreateFilter(securityContextRestriction);

    public Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext =>
        this.lazyInnerService.Value.CreateFilter(securityContextRestriction);
}

public class PermissionFilterFactory<TPermission, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory,
    IPermissionRestrictionTypeFilterFactory<TPermissionRestriction> permissionRestrictionTypeFilterFactory) : IPermissionFilterFactory<TPermission>
    where TPermissionRestriction : class
{
    public Expression<Func<TPermission, bool>> CreateFilter(SecurityContextRestriction securityContextRestriction)
    {
        return new Func<SecurityContextRestriction<ISecurityContext>, Expression<Func<TPermission, bool>>>(this.CreateFilter)
            .CreateGenericMethod(securityContextRestriction.SecurityContextType)
            .Invoke<Expression<Func<TPermission, bool>>>(this, securityContextRestriction);
    }

    public Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        if (securityContextRestriction.Filter != null)
        {
            var restrictionFilter = this.CreateFilter(permissionRestrictionFilterFactory.CreateFilter(securityContextRestriction.Filter));

            if (securityContextRestriction.Required)
            {
                return restrictionFilter;
            }
            else
            {
                return this.CreateFilter<TSecurityContext>().Not().BuildOr(restrictionFilter);
            }
        }
        else
        {
            if (securityContextRestriction.Required)
            {
                return this.CreateFilter<TSecurityContext>();
            }
            else
            {
                return _ => true;
            }
        }
    }

    private Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext =>
        this.CreateFilter(permissionRestrictionTypeFilterFactory.CreateFilter<TSecurityContext>());

    private Expression<Func<TPermission, bool>> CreateFilter(Expression<Func<TPermissionRestriction, bool>> permissionRestrictionFilter)
    {
        var permissionRestrictionQueryable = queryableSource
            .GetQueryable<TPermissionRestriction>()
            .Where(permissionRestrictionFilter)
            .Select(restrictionBindingInfo.Permission.Path);

        return permission => permissionRestrictionQueryable.Contains(permission);
    }
}