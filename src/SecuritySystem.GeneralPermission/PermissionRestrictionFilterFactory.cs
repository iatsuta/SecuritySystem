using CommonFramework;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionFilterFactory<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionFilterFactory<,,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType);

        return (IPermissionRestrictionFilterFactory<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            restrictionBindingInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.CreateFilter(restrictionFilterInfo);
    }
}

public class PermissionRestrictionFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextSource securityContextSource,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityContextObjectIdentConverter,
    IPermissionRestrictionTypeFilterFactory<TPermissionRestriction> permissionRestrictionTypeFilterFactory)
    : IPermissionRestrictionFilterFactory<TPermissionRestriction>

    where TSecurityContextObjectIdent : notnull
{
    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var baseFilter = permissionRestrictionTypeFilterFactory.CreateFilter<TSecurityContext>();

        if (restrictionFilterInfo == null)
        {
            return baseFilter;
        }
        else
        {
            var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext>();

            return new Func<SecurityContextRestrictionFilterInfo<ISecurityContext>, IdentityInfo<ISecurityContext, Ignore>,
                    Expression<Func<TPermissionRestriction, bool>>>(this.CreateRestrictionFilter)
                .CreateGenericMethod(typeof(TSecurityContext), identityInfo.IdentityType)
                .Invoke<Expression<Func<TPermissionRestriction, bool>>>(this, restrictionFilterInfo, identityInfo);
        }
    }

    public Expression<Func<TPermissionRestriction, bool>> CreateRestrictionFilter<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo,
        IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        var convertExpr = securityContextObjectIdentConverter.GetConvertExpression<TSecurityContextIdent>();

        var securityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.Id.Path).Select(convertExpr);

        return restrictionBindingInfo.SecurityContextObjectId.Path.Select(i => securityContextQueryable.Contains(i));
    }
}