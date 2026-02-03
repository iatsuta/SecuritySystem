using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionFilterFactory<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var innerServiceType = typeof(PermissionRestrictionFilterFactory<,,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPermissionRestrictionFilterFactory<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.CreateFilter(restrictionFilterInfo);
    }
}

public class PermissionRestrictionFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> restrictionBindingInfo,
    IIdentityInfoSource identityInfoSource,
    ISecurityContextSource securityContextSource,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityContextObjectIdentConverter)
    : IPermissionRestrictionFilterFactory<TPermissionRestriction>

    where TSecurityContextObjectIdent : notnull
{
    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>(SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext>();

        return new Func<SecurityContextRestrictionFilterInfo<ISecurityContext>, IdentityInfo<ISecurityContext, Ignore>,
                Expression<Func<TPermissionRestriction, bool>>>(this.CreateRestrictionFilter)
            .CreateGenericMethod(typeof(TSecurityContext), identityInfo.IdentityType)
            .Invoke<Expression<Func<TPermissionRestriction, bool>>>(this, restrictionFilterInfo, identityInfo);
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