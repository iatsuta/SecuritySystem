using System.Collections.Concurrent;
using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionTypeFilterFactory<TPermissionRestriction>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource) : IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermissionRestriction(typeof(TPermissionRestriction));

        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(restrictionBindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(PermissionRestrictionTypeFilterFactory<,,>).MakeGenericType(
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            securityContextTypeIdentityInfo.IdentityType);

        return serviceProxyFactory.Create<IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>>(
            innerServiceType,
            restrictionBindingInfo,
            securityContextTypeIdentityInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.CreateFilter<TSecurityContext>();
    }
}

public class PermissionRestrictionTypeFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextTypeIdent>(
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType> restrictionBindingInfo,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentConverter,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo)
    : IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>

    where TSecurityContextTypeIdent : notnull
{
    private readonly ConcurrentDictionary<Type, LambdaExpression> cache = new();

    public Expression<Func<TPermissionRestriction, bool>> CreateFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext
    {
        return (Expression<Func<TPermissionRestriction, bool>>)cache.GetOrAdd(typeof(TSecurityContext), _ =>
        {
            var securityContextTypeId = securityContextTypeIdentConverter.Convert(securityContextInfoSource.GetSecurityContextInfo<TSecurityContext>().Identity)
                .Id;

            var isSecurityContextTypeExpr = ExpressionHelper.GetEqualityWithExpr(securityContextTypeId);

            return restrictionBindingInfo.SecurityContextType.Path.Select(securityContextTypeIdentityInfo.Id.Path)
                .Select(isSecurityContextTypeExpr);
        });
    }
}