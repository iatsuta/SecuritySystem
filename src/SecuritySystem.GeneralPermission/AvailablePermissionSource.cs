using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using SecuritySystem.Credential;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class AvailablePermissionSource<TPrincipal, TPermission, TSecurityContextObjectIdent>(
    IQueryableSource queryableSource,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential defaultSecurityRuleCredential)
    : IAvailablePermissionSource<TPermission>
    where TPermission : class
{
    public AvailablePermissionFilter<TSecurityContextObjectIdent> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        var restrictionFiltersRequest =

            from securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions()

            where securityContextRestriction.RawFilter != null

            let filter = this.GetRestrictionFilter(securityContextRestriction.RawFilter!)

            let securityContextType = securityContextInfoSource.GetSecurityContextInfo(securityContextRestriction.SecurityContextType)

            select (securityContextType.Id, (!securityContextRestriction.Required, filter));


        return new AvailablePermissionFilter(timeProvider.GetToday())
               {
                   PrincipalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential),
                   SecurityRoleIdents = securityRolesIdentsResolver.Resolve(securityRule).ToList(),
                   RestrictionFilters = restrictionFiltersRequest.ToDictionary()
               };
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
        var filter = this.CreateFilter(securityRule);

        return this.GetAvailablePermissionsQueryable(filter);
    }

    public IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter filter)
    {
        return queryableSource.GetQueryable<TPermission>().Where(this.ToFilterExpression(filter));
    }


    public Expression<Func<TPermission, bool>> ToFilterExpression(AvailablePermissionFilter filter)
    {
        return this.GetFilterExpressionElements(filter).BuildAnd();
    }

    public IEnumerable<Expression<Func<TPermission, bool>>> GetFilterExpressionElements(AvailablePermissionFilter filter)
    {
        yield return permission => permission.Period.Contains(today);

        if (filter.PrincipalName != null)
        {
            yield return permission => this.PrincipalName == permission.TPrincipal.Name;
        }

        if (this.SecurityRoleIdents != null)
        {
            yield return permission => this.SecurityRoleIdents.Contains(permission.Role.Id);
        }

        foreach (var (securityContextTypeId, (allowGrandAccess, restrictionFilterExpr)) in this.RestrictionFilters)
        {
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
    }
}
