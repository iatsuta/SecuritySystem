using CommonFramework;
using CommonFramework.DependencyInjection;
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
        where TSecurityContext : class, ISecurityContext =>
        this.CreateRequiredFilter(securityContextRestriction).BuildAnd(this.CreateRestrictionFilter(securityContextRestriction));

    public Expression<Func<TPermission, bool>> CreateRestrictionFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        if (securityContextRestriction.Filter != null)
        {
            var permissionQueryable = queryableSource
                .GetQueryable<TPermissionRestriction>()
                .Where(permissionRestrictionFilterFactory.CreateFilter(securityContextRestriction.Filter))
                .Select(restrictionBindingInfo.Permission.Path);

            return permission => permissionQueryable.Contains(permission);
        }
        else
        {
            return _ => true;
        }
    }

    public Expression<Func<TPermission, bool>> CreateRequiredFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        if (securityContextRestriction.Required)
        {
            var permissionQueryable = queryableSource
                .GetQueryable<TPermissionRestriction>()
                .Where(permissionRestrictionTypeFilterFactory.CreateFilter<TSecurityContext>())
                .Select(restrictionBindingInfo.Permission.Path);

            return permission => permissionQueryable.Contains(permission);
        }
        else
        {
            return _ => true;
        }
    }
}