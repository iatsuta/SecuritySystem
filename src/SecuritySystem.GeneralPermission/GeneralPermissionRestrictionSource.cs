using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionRestrictionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent, TSecurityContext, TSecurityContextIdent>(
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory,
    ISecurityIdentityConverter<TSecurityContextIdent> securityContextIdentConverter,
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?> restrictionFilterInfoWrapper)

    : IPermissionRestrictionSource<TPermission, TSecurityContextIdent>
    where TPrincipal : class
    where TPermission : class
    where TSecurityRole : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull

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
                .Where(restriction => ee.Evaluate(bindingInfo.Permission.Path, restriction) == permission)
                .Where(restrictionFilter)
                .Select(restriction => ee.Evaluate(convertExpr, ee.Evaluate(bindingInfo.SecurityContextObjectId.Path, restriction)));
        });
    }
}