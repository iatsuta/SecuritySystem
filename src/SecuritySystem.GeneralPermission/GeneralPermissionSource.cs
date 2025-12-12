using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using HierarchicalExpand;

using SecuritySystem.ExternalSystem;
using SecuritySystem.GeneralPermission.Validation;

using System.Linq.Expressions;


namespace SecuritySystem.GeneralPermission;

//public interface ISecurityIdentityExtractor<TDomainObject>
//{
//    SecurityIdentity GetSecurityIdentity();
//}

//public class SecurityIdentityExtractor<TDomainObject>
//{
//    SecurityIdentity GetSecurityIdentity();
//}

public class GeneralPermissionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    IAvailablePermissionSource<TPermission, TSecurityContextObjectIdent> availablePermissionSource,
    IRawPermissionConverter<TPermissionRestriction> rawPermissionConverter,
    IQueryableSource queryableSource,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>

	where TPrincipal : class
	where TPermission : class
	where TSecurityRole : class
	where TPermissionRestriction : class
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
    public bool HasAccess()
    {
        return this.GetPermissionQuery().Any();
    }

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
        return availablePermissionSource
            .GetAvailablePermissionsQueryable(securityRule)
            .GroupJoin(queryableSource.GetQueryable<TPermissionRestriction>(), v => v, bindingInfo.Permission.Path,
                (_, restrictions) => new { restrictions })
            .ToList()
            .Select(pair => rawPermissionConverter.ConvertPermission(securityRule, pair.restrictions.ToList(), securityContextTypes))
            .ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery()
    {
        return this.GetSecurityPermissions(availablePermissionSource.CreateFilter(securityRule: securityRule));
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter)
    {
	    var availableFilter = availablePermissionSource.CreateFilter(securityRule with { CustomCredential = new SecurityRuleCredential.AnyUserCredential() });

	    return this.GetSecurityPermissions(availableFilter).Where(permissionFilter).Select(bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path));
    }

    private IQueryable<TPermission> GetSecurityPermissions(AvailablePermissionFilter<TSecurityContextObjectIdent> availablePermissionFilter)
    {
	    return queryableSource.GetQueryable<TPermission>().Where(availablePermissionSource.ToFilterExpression(availablePermissionFilter));
    }
}
