using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission;
using SecuritySystem.Services;
using AuthGeneral = ExampleApp.Domain.Auth.General;

namespace ExampleApp.Infrastructure.DependencyInjection;

public class ExtendedPermissionManagementService(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource,
    IPermissionBindingInfoSource bindingInfoSource,
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource,
    IGenericRepository genericRepository) :
    PermissionManagementService<AuthGeneral.Principal, AuthGeneral.Permission, AuthGeneral.PermissionRestriction>(serviceProxyFactory, identityInfoSource, bindingInfoSource,
        generalBindingInfoSource, restrictionBindingInfoSource)
{
    private const string ExtendedKey = nameof(AuthGeneral.Permission.ExtendedValue);

    public override async Task<PermissionData<AuthGeneral.Permission, AuthGeneral.PermissionRestriction>> CreatePermissionAsync(
        AuthGeneral.Principal dbPrincipal,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken)
    {
        var baseResult = await base.CreatePermissionAsync(dbPrincipal, managedPermission, cancellationToken);

        if (managedPermission.ExtendedData.TryGetValue(ExtendedKey, out var extendedValue))
        {
            baseResult.Permission.ExtendedValue = (string)extendedValue;

            await genericRepository.SaveAsync(baseResult.Permission, cancellationToken);
        }

        return baseResult;
    }

    public override async Task<ManagedPermission> ToManagedPermissionAsync(AuthGeneral.Permission dbPermission, CancellationToken cancellationToken)
    {
        var baseResult = await base.ToManagedPermissionAsync(dbPermission, cancellationToken);

        return baseResult.WithExtendedData(ExtendedKey, dbPermission.ExtendedValue);
    }

    public override async Task<(PermissionData<AuthGeneral.Permission, AuthGeneral.PermissionRestriction> PermissonData, bool Updated)> UpdatePermission(
        AuthGeneral.Permission dbPermission, ManagedPermission managedPermission, CancellationToken cancellationToken)
    {
        var baseResult = await base.UpdatePermission(dbPermission, managedPermission, cancellationToken);

        if (managedPermission.ExtendedData.TryGetValue(ExtendedKey, out var extendedValue) && (string)extendedValue != dbPermission.ExtendedValue)
        {
            throw new InvalidOperationException($"{ExtendedKey} can't be changed");
        }

        return baseResult;
    }
}