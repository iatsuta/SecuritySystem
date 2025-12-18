using CommonFramework;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission.AvailableSecurity;

public class AvailablePermissionFilterFactory<TPermission>(
    IServiceProvider serviceProvider,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly Lazy<IAvailablePermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(AvailablePermissionFilterFactory<,>).MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType);

        return (IAvailablePermissionFilterFactory<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            principalVisualIdentityInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.lazyInnerService.Value.CreateFilter(securityRule);
}

public class AvailablePermissionFilterFactory<TPrincipal, TPermission>(
    GeneralPermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    IPermissionSecurityRoleFilterFactory<TPermission> permissionSecurityRoleFilterFactory,
    IPermissionFilterFactory<TPermission> permissionFilterFactory,
    SecurityRuleCredential defaultSecurityRuleCredential,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo) : IAvailablePermissionFilterFactory<TPermission>
{
    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.GetFilterElements(securityRule).BuildAnd();

    private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterElements(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (bindingInfo.PermissionPeriod != null)
        {
            var today = timeProvider.GetUtcNow().Date;

            yield return

                from period in bindingInfo.PermissionPeriod.Path

                select period.StartDate <= today && (period.EndDate == null || today <= period.EndDate);
        }

        var principalName = userNameResolver.Resolve(securityRule.CustomCredential ?? defaultSecurityRuleCredential);

        if (principalName != null)
        {
            yield return bindingInfo.Principal.Path.Select(principalVisualIdentityInfo.Name.Path).Select(name => name == principalName);
        }

        foreach (var (securityRoleIdentType, securityRoleIdents) in securityRolesIdentsResolver.Resolve(securityRule))
        {
            yield return permissionSecurityRoleFilterFactory.CreateFilter(securityRoleIdentType, securityRoleIdents);
        }

        foreach (var securityContextRestriction in securityRule.GetSafeSecurityContextRestrictions())
        {
            yield return permissionFilterFactory.CreateFilter(securityContextRestriction);
        }
    }
}