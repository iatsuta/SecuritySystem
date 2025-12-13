using CommonFramework;
using CommonFramework.GenericRepository;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class PermissionFilterFactory<TPermission> : IPermissionFilterFactory<TPermission>
{
    public Expression<Func<TPermission, bool>> GetSecurityContextFilter(SecurityContextRestriction securityContextRestriction)
    {
        throw new NotImplementedException();
    }

    public Expression<Func<TPermission, bool>> GetSecurityContextFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        throw new NotImplementedException();
    }
}

public class PermissionFilterFactory<TPermission, TPermissionRestriction>(
    IServiceProvider serviceProvider,
    IQueryableSource queryableSource,
    IPermissionRestrictionFilterFactory<TPermissionRestriction> permissionRestrictionFilterFactory
) : IPermissionFilterFactory<TPermission>
    where TPermissionRestriction : class
{
    public Expression<Func<TPermission, bool>> GetSecurityContextFilter(SecurityContextRestriction securityContextRestriction)
    {
        throw new NotImplementedException();
    }

    public Expression<Func<TPermission, bool>> GetSecurityContextFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {
        var typeFilter = permissionRestrictionFilterFactory.GetFilter(securityContextRestriction);

        var restrictionFilter = securityContextRestriction.Filter == null ? _ => true : securityContextRestriction.Filter.GetPureFilter(serviceProvider);

        var requiredFilter = securityContextRestriction.Required ? queryableSource.GetQueryable<TPermissionRestriction>().an

        throw new NotImplementedException();
    }

    private Expression<Func<TPermissionRestriction, bool>> GetRestrictionFilter<TSecurityContext>(
        SecurityContextRestriction<TSecurityContext> securityContextRestriction)
        where TSecurityContext : class, ISecurityContext
    {

        var restrictionFilter = permissionRestrictionFilterFactory.GetFilter(securityContextRestriction.RawFilter);

        var allowGrandAccess = !securityContextRestriction.Required;

        if (allowGrandAccess)
        {

        }

        var baseFilter =
            ExpressionEvaluateHelper.InlineEvaluate(ee =>
                ExpressionHelper
                    .Create((TPermission permission) =>
                        permission.Restrictions.Any(r => r.SecurityContextType.Id
                                                         == securityContextTypeId
                                                         && ee.Evaluate(
                                                             restrictionFilterExpr,
                                                             r.SecurityContextId))));

        if (allowGrandAccess)
        {
            var grandAccessExpr = ExpressionHelper.Create(
                (TPermission permission) =>
                    permission.Restrictions.All(r => r.SecurityContextType.Id != securityContextTypeId));

            yield return baseFilter.BuildOr(grandAccessExpr);
        }
        else
        {
            yield return baseFilter;
        }
    }


    //public AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    //{
    //    var restrictionFiltersRequest =

    //        from securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions()

    //        where securityContextRestriction.RawFilter != null

    //        let filter = this.GetRestrictionFilter(securityContextRestriction.RawFilter!)

    //        let securityContextType = securityContextInfoSource.GetSecurityContextInfo(securityContextRestriction.SecurityContextType)

    //        select (securityIdentityConverter.Convert(securityContextType.Identity).Id, (!securityContextRestriction.Required, filter));


    //    return new AvailablePermissionFilter<TSecurityContextObjectIdent>()
    //    {
    //        Date = timeProvider.GetUtcNow().Date,
    //        PrincipalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential),
    //        SecurityRoleIdents = securityRolesIdentsResolver.Resolve(securityRule),
    //        RestrictionFilters = restrictionFiltersRequest.ToDictionary()
    //    };
    //}

    private Expression<Func<TSecurityContextObjectIdent, bool>> GetRestrictionFilter(SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<SecurityContextRestrictionFilterInfo<ISecurityContext>, Expression<Func<TSecurityContextObjectIdent, bool>>>(this.GetRestrictionFilterExpression)
               .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
               .Invoke<Expression<Func<TSecurityContextObjectIdent, bool>>>(this, restrictionFilterInfo);
    }

    private Expression<Func<TSecurityContextObjectIdent, bool>> GetRestrictionFilterExpression<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                                                                    .Select(identityInfo.Id.Path);

        return securityContextId => filteredSecurityContextQueryable.Contains(securityContextId);
    }
}