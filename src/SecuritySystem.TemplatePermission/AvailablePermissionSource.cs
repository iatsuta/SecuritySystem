using System.Linq.Expressions;

using CommonFramework;
using SecuritySystem.Attributes;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class AvailablePermissionSource(
    [DisabledSecurity] IRepository<TPermission> permissionRepository,
    TimeProvider timeProvider,
    IUserNameResolver userNameResolver,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    SecurityRuleCredential defaultSecurityRuleCredential)
    : IAvailablePermissionSource
{
    public AvailablePermissionFilter CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule)
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

    private Expression<Func<Guid, bool>> GetRestrictionFilter(SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<SecurityContextRestrictionFilterInfo<ISecurityContext>, Expression<Func<Guid, bool>>>(this.GetRestrictionFilterExpression)
               .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
               .Invoke<Expression<Func<Guid, bool>>>(this, restrictionFilterInfo);
    }

    private Expression<Func<Guid, bool>> GetRestrictionFilterExpression<TSecurityContext>(
        SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, Guid>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo)
                                                                    .Select(identityInfo.IdPath);

        return securityContextId => filteredSecurityContextQueryable.Contains(securityContextId);
    }

    public IQueryable<TPermission> GetAvailablePermissionsQueryable(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        var filter = this.CreateFilter(securityRule);

        return this.GetAvailablePermissionsQueryable(filter);
    }

    public IQueryable<TPermission> GetAvailablePermissionsQueryable(AvailablePermissionFilter filter)
    {
        return permissionRepository.GetQueryable().Where(filter.ToFilterExpression());
    }
}
