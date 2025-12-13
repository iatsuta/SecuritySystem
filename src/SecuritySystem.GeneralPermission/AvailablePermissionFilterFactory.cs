using CommonFramework;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.Services;

using System.Linq.Expressions;

namespace SecuritySystem.GeneralPermission;

public class AvailablePermissionFilterFactory<TPermission>(IServiceProvider serviceProvider, GeneralPermissionBindingInfo bindingInfo) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly Lazy<IAvailablePermissionFilterFactory<TPermission>> lazyInnerService = new(() =>
    {
        var innerServiceType = typeof(AvailablePermissionFilterFactory<,>).MakeGenericType(bindingInfo.PrincipalType, typeof(TPermission));

        return (IAvailablePermissionFilterFactory<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            bindingInfo);
    });

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.lazyInnerService.Value.CreateFilter(securityRule);
}

public class AvailablePermissionFilterFactory<TPrincipal, TPermission>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission> bindingInfo,
    TimeProvider timeProvider,
    IUserNameResolver<TPrincipal> userNameResolver,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    ISecurityRolesIdentsResolver securityRolesIdentsResolver,
    IPermissionSecurityRoleFilterFactory<TPermission> permissionSecurityRoleFilterFactory,
    IPermissionFilterFactory<TPermission> permissionFilterFactory,
    SecurityRuleCredential defaultSecurityRuleCredential) : IAvailablePermissionFilterFactory<TPermission>
{
    private readonly VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo<TPrincipal>();

    public Expression<Func<TPermission, bool>> CreateFilter(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.GetFilterElements(securityRule).BuildAnd();

    private IEnumerable<Expression<Func<TPermission, bool>>> GetFilterElements(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        if (bindingInfo.Period != null)
        {
            var today = timeProvider.GetUtcNow().Date;

            yield return

                from period in bindingInfo.Period.Path

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
            yield return permissionFilterFactory.GetSecurityContextFilter(securityContextRestriction);
        }
    }
}