using CommonFramework;
using CommonFramework.IdentitySource;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class GeneralPermissionSystem<TPermission>(
    IServiceProvider serviceProvider,
    IIdentityInfoSource identityInfoSource,
    IGeneralPermissionBindingInfoSource bindingInfoSource,
    SecurityRuleCredential securityRuleCredential) : IPermissionSystem<TPermission>
{
    private readonly Lazy<IPermissionSystem<TPermission>> lazyInnerService = new(() =>
    {
        var generalBindingInfo = bindingInfoSource.GetForPermission(typeof(TPermission));

        var securityRoleIdentityInfo = identityInfoSource.GetIdentityInfo(generalBindingInfo.SecurityRoleType);

        var innerServiceType = typeof(GeneralPermissionSystem<,,>).MakeGenericType(
            generalBindingInfo.PermissionType,
            generalBindingInfo.SecurityRoleType,
            securityRoleIdentityInfo.IdentityType);

        return (IPermissionSystem<TPermission>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            generalBindingInfo,
            securityRoleIdentityInfo,
            securityRuleCredential);
    });

    private IPermissionSystem<TPermission> InnerService => this.lazyInnerService.Value;

    public Type PermissionType => this.InnerService.PermissionType;

    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull =>
        this.InnerService.GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(restrictionFilterInfo);

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        this.InnerService.GetPermissionSource(securityRule);

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) =>
        ((IPermissionSystem)this.InnerService).GetPermissionSource(securityRule);

    public Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken = default) =>
        this.InnerService.GetAvailableSecurityRoles(cancellationToken);
}

public class GeneralPermissionSystem<TPermission, TSecurityRole, TSecurityRoleIdent>(
    IServiceProvider serviceProvider,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    ISecurityRoleSource securityRoleSource,
    SecurityRuleCredential securityRuleCredential,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentityInfo)
    : IPermissionSystem<TPermission>

    where TPermission : class
    where TSecurityRole : class
    where TSecurityRoleIdent : notnull
{
    public Type PermissionType { get; } = typeof(TPermission);

    public IPermissionRestrictionSource<TPermission, TSecurityContextIdent> GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(
        SecurityContextRestrictionFilterInfo<TSecurityContext>? restrictionFilterInfo)
        where TSecurityContext : class, ISecurityContext
        where TSecurityContextIdent : notnull
    {
        return ActivatorUtilities
            .CreateInstance<GeneralPermissionRestrictionSource<TPermission, TSecurityContext, TSecurityContextIdent>>(
                serviceProvider,
                new Tuple<SecurityContextRestrictionFilterInfo<TSecurityContext>?>(restrictionFilterInfo));
    }

    public IPermissionSource<TPermission> GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return ActivatorUtilities
            .CreateInstance<GeneralPermissionSource<TPermission>>(
                serviceProvider,
                securityRule.TryApplyCredential(securityRuleCredential));
    }

    public async Task<IEnumerable<SecurityRole>> GetAvailableSecurityRoles(CancellationToken cancellationToken)
    {
        var dbRolesIdents = await availablePermissionSource
            .GetQueryable(DomainSecurityRule.AnyRole with { CustomCredential = securityRuleCredential })
            .Select(generalBindingInfo.SecurityRole.Path.Select(securityRoleIdentityInfo.Id.Path))
            .Distinct()
            .GenericToListAsync(cancellationToken);

        return dbRolesIdents.Select(ident => securityRoleSource.GetSecurityRole(TypedSecurityIdentity.Create(ident)));
    }

    IPermissionSource IPermissionSystem.GetPermissionSource(DomainSecurityRule.RoleBaseSecurityRule securityRule) => this.GetPermissionSource(securityRule);
}