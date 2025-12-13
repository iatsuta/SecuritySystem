using CommonFramework;
using CommonFramework.IdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.GeneralPermission.AvailableSecurity;

public class GeneralAvailableSecurityRoleSource(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    GeneralPermissionBindingInfo bindingInfo) : IAvailableSecurityRoleSource
{
    private readonly Lazy<IAvailableSecurityRoleSource> lazyInnerService = new(() =>
    {
        var securityRoleIdentityInfo = identityInfoSource.GetIdentityInfo(bindingInfo.SecurityRoleType);

        var innerServiceType = typeof(GeneralAvailableSecurityRoleSource<,,,>)
            .MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType, bindingInfo.SecurityRoleType, securityRoleIdentityInfo.IdentityType);

        return (IAvailableSecurityRoleSource)ActivatorUtilities.CreateInstance(serviceProvider, innerServiceType, securityRoleIdentityInfo);
    });

    public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(SecurityRuleCredential securityRuleCredential, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.GetAvailableSecurityRoles(securityRuleCredential, cancellationToken);
}

public class GeneralAvailableSecurityRoleSource<TPrincipal, TPermission, TSecurityRole, TSecurityRoleIdent>(
    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole> bindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    ISecurityRoleSource securityRoleSource,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentity) : IAvailableSecurityRoleSource
    where TSecurityRoleIdent : notnull
{
    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(SecurityRuleCredential securityRuleCredential, CancellationToken cancellationToken)
    {
        var dbRolesIdents = await availablePermissionSource
            .GetQueryable(DomainSecurityRule.AnyRole with { CustomCredential = securityRuleCredential })
            .Select(bindingInfo.SecurityRole.Path.Select(securityRoleIdentity.Id.Path))
            .Distinct()
            .GenericToListAsync(cancellationToken);

        return dbRolesIdents.Select(ident => securityRoleSource.GetSecurityRole(new SecurityIdentity<TSecurityRoleIdent>(ident)));
    }
}