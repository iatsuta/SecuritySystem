using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSource<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> bindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
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
            .GetQueryable(securityRule)
            .GroupJoin(queryableSource.GetQueryable<TPermissionRestriction>(), v => v, bindingInfo.Permission.Path,
                (_, restrictions) => new { restrictions })
            .ToList()
            .Select(pair => rawPermissionConverter.ConvertPermission(securityRule, pair.restrictions.ToList(), securityContextTypes))
            .ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery()
    {
        return availablePermissionSource.GetQueryable(securityRule);
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter)
    {
        var availableFilter = availablePermissionSource.GetQueryable(securityRule with { CustomCredential = new SecurityRuleCredential.AnyUserCredential() });

        return availableFilter.Select(bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path));
    }
}