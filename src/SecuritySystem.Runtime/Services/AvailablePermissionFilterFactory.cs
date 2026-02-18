using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.Credential;

namespace SecuritySystem.Services;

public class AvailablePermissionFilterFactory<TPermission>(
    IServiceProxyFactory serviceProxyFactory,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly Lazy<IAvailablePermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(AvailablePermissionFilterFactory<,>).MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType);

        return serviceProxyFactory.Create<IAvailablePermissionFilterFactory<TPermission>>(
            innerServiceType,
            bindingInfo,
            principalVisualIdentityInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.lazyInnerService.Value.CreateFilter(securityRule);
}

public class AvailablePermissionFilterFactory<TPrincipal, TPermission>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    IPermissionSecurityRoleIdentsFilterFactory<TPermission> permissionSecurityRoleIdentsFilterFactory,
    IPermissionFilterFactory<TPermission> permissionFilterFactory,
    SecurityRuleCredential defaultSecurityRuleCredential,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo) : IAvailablePermissionFilterFactory<TPermission>
{
    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.GetFilterElements(securityRule).BuildAnd();

    private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterElements(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (bindingInfo.PermissionStartDate != null)
        {
            yield return bindingInfo.GetPeriodFilter(timeProvider.GetUtcNow().Date);
        }

        var principalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential);

        if (principalName != null)
        {
            yield return bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path).Select(name => name == principalName);
        }

        yield return permissionSecurityRoleIdentsFilterFactory.CreateFilter(securityRule);

        foreach (var securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions())
        {
            yield return permissionFilterFactory.CreateFilter(securityContextRestriction);
        }
    }
}