using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>(
    Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?> restrictionFilterInfoWrapper)
    : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>

    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr()
    {
        throw new NotImplementedException();
    }
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