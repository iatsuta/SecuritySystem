using CommonFramework;

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
    ISecurityIdentityExtractor<TPermission> permissionSecurityIdentityExtractor) : IManagedPrincipalConverter<TPrincipal>
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
        new()
        {
            Identity = permissionSecurityIdentityExtractor.Extract(permission),
            IsVirtual = bindingInfo.IsReadonly,
            SecurityRole = permissionSecurityRoleResolver.Resolve(permission),
            Period = bindingInfo.GetSafePeriod(permission),
            Comment = bindingInfo.GetSafeComment(permission),
            Restrictions = await rawPermissionRestrictionLoader.LoadAsync(permission, cancellationToken)
        };
}