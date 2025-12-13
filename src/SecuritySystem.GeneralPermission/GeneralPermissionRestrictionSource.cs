using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionRestrictionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent, TSecurityContext, TSecurityContextIdent>(
    IServiceProvider serviceProvider,
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
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
    private readonly IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo =
        identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextIdent>();

    private readonly SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo = restrictionFilterInfoWrapper.Item1;

    public Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetIdentsExpr()
    {
        var baseFilter = this.GetBaseIdentsExpr();

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
    }

    private Expression<Func<TPermission, IEnumerable<TSecurityContextIdent>>> GetBaseIdentsExpr()
    {
        var restrictionQueryable = queryableSource.GetQueryable<TPermissionRestriction>();

        var convertExpr = securityContextIdentConverter.GetConvertExpression<TSecurityContextObjectIdent>();

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TPermission, IEnumerable<TSecurityContextIdent>>>(ee =>
        {
            var restrictionFilter = permissionRestrictionFilterFactory.GetFilter<TSecurityContext>();

            return permission => restrictionQueryable
                .Where(restriction => ee.Evaluate(bindingInfo.Permission.Path, restriction) == permission)
                .Where(restrictionFilter)
                .Select(restriction => ee.Evaluate(convertExpr, ee.Evaluate(bindingInfo.SecurityContextObjectId.Path, restriction)));
        });
    }
}