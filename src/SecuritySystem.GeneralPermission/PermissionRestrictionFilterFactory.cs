using CommonFramework;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionFilterFactory<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    GeneralPermissionBindingInfo bindingInfo) : IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(PermissionRestrictionFilterFactory<,,>).MakeGenericType(
            typeof(TPermissionRestriction),
            bindingInfo.SecurityContextTypeType,
            securityContextTypeIdentityInfo.IdentityType);

        return (IPermissionRestrictionFilterFactory<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            securityContextTypeIdentityInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.GetFilter(restrictionFilterInfo);
    }
}

public class PermissionRestrictionFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextTypeIdent>(
    ISecurityContextInfoSource securityContextInfoSource,
    IIdentityInfoSource identityInfoSource,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentConverter,
    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType> permissionRestrictionToSecurityContextTypeInfo,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo) : IPermissionRestrictionFilterFactory<TPermissionRestriction>

    where TSecurityContextTypeIdent : notnull
{
    private readonly IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo =
        identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

    public Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>(SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        if (restrictionFilterInfo == null)
        {
            return baseFilter;
        }
        else
        {
            var securityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                .Where(restrictionFilterInfo.GetPureFilter(serviceProvider))
                .Select(this.identityInfo.Id.Path);

            return baseFilter.Select(idents => idents.Where(i => securityContextQueryable.Contains(i)));
        }

        return (Expression<Func<TPermissionRestriction, bool>>)cache.GetOrAdd(typeof(TSecurityContext), _ =>
        {
            var securityContextTypeId = securityContextTypeIdentConverter.Convert(securityContextInfoSource.GetSecurityContextInfo<TSecurityContext>().Identity)
                .Id;

            var isSecurityContextTypeExpr = ExpressionHelper.GetEqualityWithExpr(securityContextTypeId);

            return permissionRestrictionToSecurityContextTypeInfo.SecurityContextType.Path.Select(securityContextTypeIdentityInfo.Id.Path)
                .Select(isSecurityContextTypeExpr);
        });
    }
}