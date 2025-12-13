using CommonFramework;
using CommonFramework.GenericRepository;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class PermissionFilterFactory<TPermission>(IServiceProvider serviceProvider, GeneralPermissionBindingInfo bindingInfo) : IPermissionFilterFactory<TPermission>
{
    private readonly Lazy<IPermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var innerServiceType = typeof(PermissionFilterFactory<,>).MakeGenericType(typeof(TPermission), bindingInfo.PermissionRestrictionType);

        return (IPermissionFilterFactory<TPermission>)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType);
    });


    public Expression<Func<TPermission, bool>> CreateFilter(SecurityContextRestriction securityContextRestriction) =>
        this.lazyInnerService.Value.CreateFilter(securityContextRestriction);

    public Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext =>
        this.lazyInnerService.Value.CreateFilter(securityContextRestriction);
}

public class PermissionFilterFactory<TPermission, TPermissionRestriction>(
    IQueryableSource queryableSource,
    IPermissionRestrictionToPermissionInfo<TPermissionRestriction, TPermission> bindingInfo,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory) : IPermissionFilterFactory<TPermission>
    where TPermissionRestriction : class
{
    public Expression<Func<TPermission, bool>> CreateFilter(SecurityContextRestriction securityContextRestriction)
    {
        return new Func<SecurityContextRestriction<ISecurityContext>, Expression<Func<TPermission, bool>>>(this.CreateFilter)
            .CreateGenericMethod(securityContextRestriction.SecurityContextType)
            .Invoke<Expression<Func<TPermission, bool>>>(this);
    }

    public Expression<Func<TPermission, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        if (securityContextRestriction.Required)
        {
            var typeFilter = permissionRestrictionFilterFactory.CreateFilter(securityContextRestriction.Filter);

            var permissionQueryable = queryableSource
                .GetQueryable<TPermissionRestriction>()
                .Where(typeFilter)
                .Select(bindingInfo.Permission.Path);

            return permission => permissionQueryable.Contains(permission);
        }
        else
        {
            return _ => true;
        }
    }
}