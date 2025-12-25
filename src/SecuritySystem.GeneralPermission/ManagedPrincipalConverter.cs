using CommonFramework;
using CommonFramework.DependencyInjection;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.GeneralPermission;

public class ManagedPrincipalConverter<TPrincipal>(IServiceProxyFactory serviceProxyFactory, IPermissionBindingInfoSource bindingInfoSource)
    : IManagedPrincipalConverter<TPrincipal>
{
    private readonly Lazy<IManagedPrincipalConverter<TPrincipal>> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoSource.GetForPrincipal(typeof(TPrincipal));

        var innerServiceType = typeof(ManagedPrincipalConverter<,>).MakeGenericType(
            bindingInfo.PrincipalType,
            bindingInfo.PermissionType);

        return serviceProxyFactory.Create<IManagedPrincipalConverter<TPrincipal>>(innerServiceType, bindingInfo);
    });

    public Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken) =>
        this.lazyInnerService.Value.ToManagedPrincipalAsync(principal, cancellationToken);
}

public class ManagedPrincipalConverter<TPrincipal, TPermission>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    IManagedPrincipalHeaderConverter<TPrincipal> headerConverter,
    IPermissionSecurityRoleResolver<TPermission> permissionSecurityRoleResolver,
    IPermissionLoader<TPrincipal, TPermission> permissionLoader,
    IRawPermissionRestrictionLoader<TPermission> rawPermissionRestrictionLoader,
    ISecurityIdentityExtractorFactory securityIdentityExtractorFactory) : IManagedPrincipalConverter<TPrincipal>
    where TPrincipal : class
    where TPermission : class
{
    public async Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken)
    {
        var permissions = await permissionLoader.LoadAsync(principal, cancellationToken);

        return new ManagedPrincipal(
            headerConverter.Convert(principal),
            await permissions.SyncWhenAll(permission => this.ToManagedPermissionAsync(permission, cancellationToken)));
    }

    private async Task<ManagedPermission> ToManagedPermissionAsync(TPermission permission, CancellationToken cancellationToken) =>
        new(
            securityIdentityExtractorFactory.Create<TPermission>().Extract(permission),
            bindingInfo.IsReadonly,
            permissionSecurityRoleResolver.Resolve(permission),
            bindingInfo.GetSafePeriod(permission),
            bindingInfo.GetSafeComment(permission),
            await rawPermissionRestrictionLoader.LoadAsync(permission, cancellationToken));
}