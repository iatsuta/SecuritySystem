using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>(
    IServiceProvider serviceProvider,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource,
    Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?> restrictionFilterInfoWrapper)
    : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>

    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    private readonly Lazy<IPermissionRestrictionSource<TPermission, TSecurityContextIdent>> lazyInnerService = new(() =>
    {
        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(typeof(TPermission));

        var innerServiceType = typeof(GeneralPermissionRestrictionSource<,,,,,>).MakeGenericType(

            restrictionBindingInfo.PermissionType,
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType,
            typeof(TSecurityContext),
            typeof(TSecurityContextIdent));

        return (IPermissionRestrictionSource<TPermission, TSecurityContextIdent>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            restrictionBindingInfo,
            restrictionFilterInfoWrapper);
    });

    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr() => this.lazyInnerService.Value.GetIdentsExpr();
}

public class GeneralPermissionRestrictionSource<TPermission, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent, TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory,
    ISecurityIdentityConverter<TSecurityContextIdent> securityContextIdentConverter,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?> restrictionFilterInfoWrapper)
    : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>

    where TPermission : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    private readonly SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo = restrictionFilterInfoWrapper.Item1;

    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr()
    {
        var restrictionFilter = permissionRestrictionFilterFactory.CreateFilter(this.restrictionFilterInfo);

        var restrictionQueryable = queryableSource.GetQueryable<TPermissionRestriction>();

        var convertExpr = securityContextIdentConverter.GetConvertExpression<TSecurityContextObjectIdent>();

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TPermission, IEnumerable<TSecurityContextIdent>>>(ee =>
        {
            return permission => restrictionQueryable
                .Where(restriction => ee.Evaluate(restrictionBindingInfo.Permission.Path, restriction) == permission)
                .Where(restrictionFilter)
                .Select(restriction => ee.Evaluate(convertExpr, ee.Evaluate(restrictionBindingInfo.SecurityContextObjectId.Path, restriction)));
        });
    }
}