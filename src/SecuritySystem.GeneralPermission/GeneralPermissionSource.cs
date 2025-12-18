using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.ExternalSystem;

using System.Linq.Expressions;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSource<TPermission>(
    IServiceProvider serviceProvider,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource bindingRestrictionInfoSource,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>
{
    private readonly Lazy<IPermissionSource<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var bindingRestrictionInfo = bindingRestrictionInfoSource.GetForPermission(typeof(TPermission));

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(GeneralPermissionSource<,,,,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType,
            bindingRestrictionInfo.PermissionRestrictionType,
            bindingRestrictionInfo.SecurityContextTypeType,
            bindingRestrictionInfo.SecurityContextObjectIdentType);

        return (IPermissionSource<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
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

public class GeneralPermissionSource<TPrincipal, TPermission, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    IRawPermissionConverter<TPermissionRestriction> rawPermissionConverter,
    IQueryableSource queryableSource,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo,
    DomainSecurityRule.RoleBaseSecurityRule securityRule) : IPermissionSource<TPermission>

    where TPrincipal : class
    where TPermission : class
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
            .GroupJoin(queryableSource.GetQueryable<TPermissionRestriction>(), v => v, restrictionBindingInfo.Permission.Path,
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