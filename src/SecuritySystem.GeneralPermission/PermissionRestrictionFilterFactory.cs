using CommonFramework;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionFilterFactory<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource bindingInfoSource) : IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(PermissionRestrictionFilterFactory<,,,>).MakeGenericType(
            typeof(TPermissionRestriction),
            bindingInfo.SecurityContextTypeType,
            bindingInfo.SecurityContextObjectIdentType,
            securityContextTypeIdentityInfo.IdentityType);

        return (IPermissionRestrictionFilterFactory<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            securityContextTypeIdentityInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.CreateFilter(restrictionFilterInfo);
    }
}

public class PermissionRestrictionFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    IIdentityInfoSource identityInfoSource,
    ISecurityContextSource securityContextSource,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityContextObjectIdentConverter,
    IPermissionRestrictionTypeFilterFactory<TPermissionRestriction> permissionRestrictionTypeFilterFactory,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo)
    : IPermissionRestrictionFilterFactory<TPermissionRestriction>

    where TSecurityContextTypeIdent : notnull
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

        return bindingInfo.SecurityContextObjectId.Path.Select(i => securityContextQueryable.Contains(i));
    }
}