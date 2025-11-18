using System.Linq.Expressions;

using CommonFramework;

using Framework.Authorization.Domain;
using Framework.DomainDriven.Repository;

using GenericQueryable.Fetching;

using SecuritySystem;
using SecuritySystem.Attributes;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationPermissionSource<TPermission>(
    IAvailablePermissionSource availablePermissionSource,
    IRealTypeResolver realTypeResolver,
    [DisabledSecurity] IRepository<TPermission> permissionRepository,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
{
    public bool HasAccess()
    {
        return this.GetPermissionQuery().Any();
    }

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
        var permissions = availablePermissionSource.GetAvailablePermissionsQueryable(securityRule)
                                                   .WithFetch(r => r.Fetch(v => v.Restrictions).ThenFetch(v => v.SecurityContextType))
                                                   .ToList();

        return permissions
               .Select(permission => this.ConvertPermission(permission, securityContextTypes))
               .ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery()
    {
        return this.GetSecurityPermissions(availablePermissionSource.CreateFilter(securityRule: securityRule));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter)
    {
        return this.GetSecurityPermissions(
                       availablePermissionSource.CreateFilter(securityRule with { CustomCredential = new SecurityRuleCredential.AnyUserCredential() }))
                   .Where(permissionFilter)
                   .Select(permission => permission.Principal.Name);
    }

    private IQueryable<TPermission> GetSecurityPermissions(AvailablePermissionFilter availablePermissionFilter)
    {
        return permissionRepository.GetQueryable()
                                   .Where(availablePermissionFilter.ToFilterExpression());
    }

    private Dictionary<Type, Array> ConvertPermission(
        TPermission permission,
        IEnumerable<Type> securityContextTypes)
    {
        var purePermission = permission.Restrictions.GroupBy(
                                           permissionRestriction => permissionRestriction.SecurityContextType.Id,
                                           permissionRestriction => permissionRestriction.SecurityContextId)
                                       .ToDictionary(g => g.Key, g => g.ToList());

        var filterInfoDict = securityRule.GetSafeSecurityContextRestrictionFilters().ToDictionary(filterInfo => filterInfo.SecurityContextType);

        return securityContextTypes.ToDictionary(
            securityContextType => securityContextType,
            Array (securityContextType) =>
            {
                var securityContextRestrictionFilterInfo = filterInfoDict.GetValueOrDefault(securityContextType);

                var securityContextTypeId =
                    securityContextInfoSource.GetSecurityContextInfo(realTypeResolver.Resolve(securityContextType)).Id;

                var baseIdents = purePermission.GetValueOrDefault(securityContextTypeId, []);

                if (securityContextRestrictionFilterInfo == null)
                {
                    return baseIdents.ToArray();
                }
                else
                {
                    return this.ApplySecurityContextFilter(baseIdents, securityContextRestrictionFilterInfo);
                }

            });
    }

    private Guid[] ApplySecurityContextFilter(List<Guid> securityContextIdents, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<List<Guid>, SecurityContextRestrictionFilterInfo<ISecurityContext>, Guid[]>(this.ApplySecurityContextFilter)
               .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
               .Invoke<Guid[]>(this, securityContextIdents, restrictionFilterInfo);
    }

    private Guid[] ApplySecurityContextFilter<TSecurityContext>(List<Guid> baseSecurityContextIdents, SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, Guid>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.IdPath);

        if (baseSecurityContextIdents.Any())
        {
            return filteredSecurityContextQueryable.Where(securityContextId => baseSecurityContextIdents.Contains(securityContextId))
                                                   .ToArray();
        }
        else
        {
            return filteredSecurityContextQueryable.ToArray();
        }
    }
}
