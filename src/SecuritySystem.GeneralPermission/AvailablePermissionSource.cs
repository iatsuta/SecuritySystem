using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using SecuritySystem.Credential;

using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class AvailablePermissionSource<TPrincipal, TPermission, TSecurityContextObjectIdent>(
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityIdentityConverter,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential defaultSecurityRuleCredential)
    : IAvailablePermissionSource<TPermission>
    where TPermission : class
    where TSecurityContextObjectIdent : notnull
{
    public AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
	    throw new NotImplementedException();
	    //var restrictionFiltersRequest =

	    // from securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions()

	    // where securityContextRestriction.RawFilter != null

	    // let filter = this.GetRestrictionFilter(securityContextRestriction.RawFilter!)

	    // let securityContextType = securityContextInfoSource.GetSecurityContextInfo(securityContextRestriction.SecurityContextType)

	    // select (securityIdentityConverter.Convert(securityContextType.Identity).Id, (!securityContextRestriction.Required, filter));


	    //   return new AvailablePermissionFilter<TSecurityContextObjectIdent>()
	    //          {
	    //           Date = timeProvider.GetUtcNow().Date,
	    //  PrincipalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential),
	    //              SecurityRoleIdents = securityRolesIdentsResolver.Resolve(securityRule),
	    //              RestrictionFilters = restrictionFiltersRequest.ToDictionary()
	    //          };
    }

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

    public IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
	{
		throw new NotImplementedException();

		//var filter = this.CreateFilter(securityRule);

        //return this.GetAvailablePermissionsQueryable(filter);
    }

    //public IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter<TSecurityContextObjectIdent> filter)
    //{
    //    return queryableSource.GetQueryable<TPermission>().Where(this.ToFilterExpression(filter));
    //}


    //private Expression<Func<TPermission, bool>> ToFilterExpression(AvailablePermissionFilter<TSecurityContextObjectIdent> filter)
    //{
    //    return this.GetFilterExpressionElements(filter).BuildAnd();
    //}

    //private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterExpressionElements(AvailablePermissionFilter<TSecurityContextObjectIdent> filter)
    //{
    //    yield return permission => permission.Period.Contains(today);

    //    if (filter.PrincipalName != null)
    //    {
    //        yield return permission => filter.PrincipalName == permission.TPrincipal.Name;
    //    }

    //    if (filter.SecurityRoleIdents != null)
    //    {
    //        yield return permission => this.SecurityRoleIdents.Contains(permission.Role.Id);
    //    }

    //    foreach (var (securityContextTypeId, (allowGrandAccess, restrictionFilterExpr)) in filter.RestrictionFilters)
    //    {
    //        var baseFilter =
    //            ExpressionEvaluateHelper.InlineEvaluate(ee =>
    //                                                        ExpressionHelper
    //                                                            .Create((TPermission permission) =>
    //                                                                        permission.Restrictions.Any(r => r.SecurityContextType.Id
    //                                                                                    == securityContextTypeId
    //                                                                                    && ee.Evaluate(
    //                                                                                        restrictionFilterExpr,
    //                                                                                        r.SecurityContextId))));

    //        if (allowGrandAccess)
    //        {
    //            var grandAccessExpr = ExpressionHelper.Create(
    //                (TPermission permission) =>
    //                    permission.Restrictions.All(r => r.SecurityContextType.Id != securityContextTypeId));

    //            yield return baseFilter.BuildOr(grandAccessExpr);
    //        }
    //        else
    //        {
    //            yield return baseFilter;
    //        }
    //    }
    //}
}
