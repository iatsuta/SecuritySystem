using CommonFramework;

using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

using System.Linq.Expressions;

using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityContextTypeIdent>(
    GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> info,
    IAvailablePermissionSource<TPermission, TSecurityContextObjectIdent> availablePermissionSource,
    IRealTypeResolver realTypeResolver,
    IQueryableSource queryableSource,
    ISecurityContextInfoSource securityContextInfoSource,
    ISecurityContextSource securityContextSource,
    IIdentityInfoSource identityInfoSource,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo,
    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityIdentityConverter,
	UserSourceInfo<TPrincipal> userSourceInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull

	where TSecurityContextTypeIdent : notnull
{
    public bool HasAccess()
    {
        return this.GetPermissionQuery().Any();
    }


    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
	    return availablePermissionSource
		    .GetAvailablePermissionsQueryable(securityRule)
		    .GroupJoin(queryableSource.GetQueryable<TPermissionRestriction>(), v => v, info.ToPermission.Path,
			    (_, restrictions) => new { restrictions })
		    .ToList()
		    .Select(pair => this.ConvertPermission(pair.restrictions.ToList(), securityContextTypes))
		    .ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery()
    {
        return this.GetSecurityPermissions(availablePermissionSource.CreateFilter(securityRule: securityRule));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter)
    {
	    var availableFilter = availablePermissionSource.CreateFilter(securityRule with { CustomCredential = new SecurityRuleCredential.AnyUserCredential() });

	    return this.GetSecurityPermissions(availableFilter).Where(permissionFilter).Select(info.ToPrincipal.Path.Select(userSourceInfo.Name.Path));
    }

    private IQueryable<TPermission> GetSecurityPermissions(AvailablePermissionFilter<TSecurityContextObjectIdent> availablePermissionFilter)
    {
	    return queryableSource.GetQueryable<TPermission>().Where(availablePermissionSource.ToFilterExpression(availablePermissionFilter));
    }

    private Dictionary<Type, Array> ConvertPermission(
	    IReadOnlyList<TPermissionRestriction> restrictions,
	    IEnumerable<Type> securityContextTypes)
    {
	    var purePermission = restrictions.GroupBy(
			    info.ToSecurityContextType.Getter.Composite(securityContextTypeIdentityInfo.Id.Getter),
			    info.ToSecurityContextObjectId.Getter)

		    .ToDictionary(g => g.Key, g => g.ToList());

        var filterInfoDict = securityRule.GetSafeSecurityContextRestrictionFilters().ToDictionary(filterInfo => filterInfo.SecurityContextType);

        return securityContextTypes.ToDictionary(
            securityContextType => securityContextType,
            Array (securityContextType) =>
            {
                var securityContextRestrictionFilterInfo = filterInfoDict.GetValueOrDefault(securityContextType);

                var securityContextTypeIdentity =
                    securityContextInfoSource.GetSecurityContextInfo(realTypeResolver.Resolve(securityContextType)).Identity;

                var securityContextTypeId = securityIdentityConverter.Convert(securityContextTypeIdentity).Id;

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

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter(List<TSecurityContextObjectIdent> securityContextIdents, SecurityContextRestrictionFilterInfo restrictionFilterInfo)
    {
        return new Func<List<TSecurityContextObjectIdent>, SecurityContextRestrictionFilterInfo<ISecurityContext>, TSecurityContextObjectIdent[]>(this.ApplySecurityContextFilter)
               .CreateGenericMethod(restrictionFilterInfo.SecurityContextType)
               .Invoke<TSecurityContextObjectIdent[]>(this, securityContextIdents, restrictionFilterInfo);
    }

    private TSecurityContextObjectIdent[] ApplySecurityContextFilter<TSecurityContext>(List<TSecurityContextObjectIdent> baseSecurityContextIdents, SecurityContextRestrictionFilterInfo<TSecurityContext> restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TSecurityContext, TSecurityContextObjectIdent>();

        var filteredSecurityContextQueryable = securityContextSource.GetQueryable(restrictionFilterInfo).Select(identityInfo.Id.Path);

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
