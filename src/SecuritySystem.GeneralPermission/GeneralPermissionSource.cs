using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;
using CommonFramework.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSource<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
{
    private readonly Lazy<IPermissionSource<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var permissionIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.PermissionType);

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(GeneralPermissionSource<,,,,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            restrictionBindingInfo.PermissionRestrictionType,
            restrictionBindingInfo.SecurityContextTypeType,
            restrictionBindingInfo.SecurityContextObjectIdentType,
            permissionIdentityInfo.IdentityType);

        return serviceProxyFactory.Create<IPermissionSource<TPermission>>(
            innerServiceType,
            bindingInfo,
            restrictionBindingInfo,
            permissionIdentityInfo,
            principalVisualIdentityInfo,
            securityRule);
    });

    private IPermissionSource<TPermission> InnerService => this.lazyInnerService.Value;

    public bool HasAccess() => this.InnerService.HasAccess();

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes) =>
        this.InnerService.GetPermissions(securityContextTypes);

    public IQueryable<TPermission> GetPermissionQuery() => this.InnerService.GetPermissionQuery();

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter) => this.InnerService.GetAccessors(permissionFilter);

}

public class GeneralPermissionSource<TPrincipal, TPermission, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermissionIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    IRawPermissionConverter<TPermissionRestriction> rawPermissionConverter,
    IQueryableSource queryableSource,
    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>

    where TPrincipal : class
    where TPermission : class
    where TPermissionRestriction : class
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
    where TPermissionIdent : notnull
{
    public bool HasAccess()
    {
        return this.GetPermissionQuery().Any();
    }

    public List<Dictionary<Type, Array>> GetPermissions(IEnumerable<Type> securityContextTypes)
    {
        var permissionIdents = availablePermissionSource.GetQueryable(securityRule).Select(permissionIdentityInfo.Id.Path).ToList();

        var containsPermissionFilter = permissionIdentityInfo.CreateContainsFilter(permissionIdents);

        var permissionRestrictions = queryableSource
            .GetQueryable<TPermissionRestriction>()
            .Where(restrictionBindingInfo.Permission.Path.Select(containsPermissionFilter))
            .ToList();

        return permissionIdents.GroupJoin(
                permissionRestrictions, id => id, restrictionBindingInfo.Permission.Getter.Composite(permissionIdentityInfo.Id.Getter),
                (_, restrictions) => rawPermissionConverter.ConvertPermission(securityRule, restrictions, securityContextTypes))
            .ToList();
    }

    public IQueryable<TPermission> GetPermissionQuery()
    {
        return availablePermissionSource.GetQueryable(securityRule);
    }

    public IEnumerable<string> GetAccessors(Expression<Func<TPermission, bool>> permissionFilter)
    {
        var availableFilter = availablePermissionSource.GetQueryable(securityRule with { CustomCredential = new SecurityRuleCredential.AnyUserCredential() });

        return availableFilter.Where(permissionFilter).Select(bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path));
    }
}