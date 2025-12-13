using System.Collections.Concurrent;
using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionTypeFilterFactory<TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    GeneralPermissionBindingInfo bindingInfo) : IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>
{
    private readonly Lazy<IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>> lazyInnerService = new(() =>
    {
        var securityContextTypeIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityContextTypeType);

        var innerServiceType = typeof(PermissionRestrictionTypeFilterFactory<,,>).MakeGenericType(
            typeof(TPermissionRestriction),
            bindingInfo.SecurityContextTypeType,
            securityContextTypeIdentityInfo.IdentityType);

        return (IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo,
            securityContextTypeIdentityInfo);
    });

    public Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext
    {
        return this.lazyInnerService.Value.GetFilter<TSecurityContext>();
    }
}

public class PermissionRestrictionTypeFilterFactory<TPermissionRestriction, TSecurityContextType, TSecurityContextTypeIdent>(
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentConverter,
    IPermissionRestrictionToSecurityContextTypeInfo<TPermissionRestriction, TSecurityContextType> permissionRestrictionToSecurityContextTypeInfo,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo)
    : IPermissionRestrictionTypeFilterFactory<TPermissionRestriction>

    where TSecurityContextTypeIdent : notnull
{
    private readonly ConcurrentDictionary<Type, LambdaExpression> cache = new();

    public Expression<Func<TPermissionRestriction, bool>> GetFilter<TSecurityContext>()
        where TSecurityContext : class, ISecurityContext
    {
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