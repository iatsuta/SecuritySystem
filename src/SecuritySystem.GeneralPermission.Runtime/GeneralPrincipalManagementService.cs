using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission.Validation.Principal;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalManagementService(
    IServiceProxyFactory serviceProxyFactory,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IEnumerable<PermissionBindingInfo> bindingInfoList,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPrincipalManagementService
{
    private readonly Lazy<IPrincipalManagementService> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoList.Single(
            bi => !bi.IsReadonly,
            () => new InvalidOperationException("No writable management service was found"),
            () => new InvalidOperationException("Multiple writable management services were found"));

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(GeneralPrincipalManagementService<,,>)
            .MakeGenericType(bindingInfo.PrincipalType, bindingInfo.PermissionType, restrictionBindingInfo.PermissionRestrictionType);

        return serviceProxyFactory.Create<IPrincipalManagementService>(innerServiceType, principalVisualIdentityInfo);
    });

    private IPrincipalManagementService InnerService => this.lazyInnerService.Value;

    public Type PrincipalType => this.InnerService.PrincipalType;

    public Task<PrincipalData> CreatePrincipalAsync(string principalName, IEnumerable<ManagedPermission> typedPermissions, CancellationToken cancellationToken = default) =>
        this.InnerService.CreatePrincipalAsync(principalName, typedPermissions, cancellationToken);

    public Task<PrincipalData> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken) =>
        this.InnerService.UpdatePrincipalNameAsync(userCredential, principalName, cancellationToken);

    public Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default) =>
        this.InnerService.RemovePrincipalAsync(userCredential, force, cancellationToken);

    public Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(UserCredential userCredential,
        IEnumerable<ManagedPermission> typedPermissions, CancellationToken cancellationToken = default) =>
        this.InnerService.UpdatePermissionsAsync(userCredential, typedPermissions, cancellationToken);
}

public class GeneralPrincipalManagementService<TPrincipal, TPermission, TPermissionRestriction>(
    [FromKeyedServices("Root")] IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction> principalValidator,
    IGenericRepository genericRepository,
    IPrincipalDomainService<TPrincipal> principalDomainService,
    IUserSource<TPrincipal> principalUserSource,
    ISecurityIdentityExtractor<TPermission> permissionIdentityExtractor,
    IPermissionLoader<TPrincipal, TPermission> permissionLoader,
    IPermissionRestrictionLoader<TPermission, TPermissionRestriction> permissionRestrictionLoader,
    IPermissionManagementService<TPrincipal, TPermission, TPermissionRestriction> permissionManagementService,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo)
    : IPrincipalManagementService

    where TPrincipal : class, new()
    where TPermission : class, new()
    where TPermissionRestriction : class, new()
{
    public Type PrincipalType { get; } = typeof(TPrincipal);

    public async Task<PrincipalData> CreatePrincipalAsync(
        string principalName,
        IEnumerable<ManagedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        var principal = await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);

        var result = await this.UpdatePermissionsAsync(principal, [], typedPermissions, cancellationToken);

        return new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(principal,
            result.AddingItems.Cast<PermissionData<TPermission, TPermissionRestriction>>());
    }

    public async Task<PrincipalData> UpdatePrincipalNameAsync(
        UserCredential userCredential,
        string principalName,
        CancellationToken cancellationToken)
    {
        var principal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        principalVisualIdentityInfo.Name.Setter(principal, principalName);

        await genericRepository.SaveAsync(principal, cancellationToken);

        return await this.ToPrincipalData(principal, cancellationToken);
    }

    public async Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken)
    {
        var principal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        var principalData = await this.ToPrincipalData(principal, cancellationToken);

        await principalDomainService.RemoveAsync(principal, force, cancellationToken);

        return principalData;
    }

    private async Task<PrincipalData<TPrincipal, TPermission, TPermissionRestriction>> ToPrincipalData(TPrincipal dbPrincipal,
        CancellationToken cancellationToken)
    {
        var dbPermissions = await permissionLoader.LoadAsync(dbPrincipal, cancellationToken);

        var permissionsData = await dbPermissions.SyncWhenAll(async dbPermission => await this.ToPermissionData(dbPermission, cancellationToken));

        return new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(dbPrincipal, permissionsData);
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>> ToPermissionData(TPermission dbPermission,
        CancellationToken cancellationToken)
    {
        var dbRestrictions = await permissionRestrictionLoader.LoadAsync(dbPermission, cancellationToken);

        return new PermissionData<TPermission, TPermissionRestriction>(dbPermission, dbRestrictions);
    }

    public async Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(
        UserCredential userCredential,
        IEnumerable<ManagedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        var dbPrincipal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        var dbPermissions = await permissionLoader.LoadAsync(dbPrincipal, cancellationToken);

        return await this.UpdatePermissionsAsync(dbPrincipal, dbPermissions, typedPermissions, cancellationToken);
    }

    private async Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(
        TPrincipal dbPrincipal,
        List<TPermission> dbPermissions,
        IEnumerable<ManagedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        var permissionMergeResult = dbPermissions.GetMergeResult(typedPermissions, permissionIdentityExtractor.Extract,
            p => p.Identity.IsDefault ? new object() : permissionIdentityExtractor.Converter.Convert(p.Identity));

        var newPermissions = await this.CreatePermissionsAsync(dbPrincipal, permissionMergeResult.AddingItems, cancellationToken);

        var updatedPermissions = await this.UpdatePermissionsAsync(permissionMergeResult.CombineItems, cancellationToken);

        var removingPermissions = await permissionMergeResult.RemovingItems.SyncWhenAll(async oldDbPermission =>
        {
            var result = await this.ToPermissionData(oldDbPermission, cancellationToken);

            foreach (var dbRestriction in result.Restrictions)
            {
                await genericRepository.RemoveAsync(dbRestriction, cancellationToken);
            }

            await genericRepository.RemoveAsync(oldDbPermission, cancellationToken);

            return result;
        });

        await principalValidator.ValidateAsync(
            new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(dbPrincipal,
                updatedPermissions.Select(pair => pair.PermissonData).Concat(newPermissions)),
            cancellationToken);

        return new MergeResult<PermissionData, PermissionData>(
            newPermissions,
            updatedPermissions.Where(pair => pair.Updated).Select(PermissionData (pair) => pair.PermissonData).Select(v => (v, v)),
            removingPermissions);
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>[]> CreatePermissionsAsync(
        TPrincipal dbPrincipal,
        IEnumerable<ManagedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        return await typedPermissions.SyncWhenAll(managedPermission => permissionManagementService.CreatePermissionAsync(dbPrincipal, managedPermission, cancellationToken));
    }

    private async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)[]> UpdatePermissionsAsync(
        IReadOnlyList<(TPermission, ManagedPermission)> permissionPairs,
        CancellationToken cancellationToken)
    {
        return await permissionPairs.SyncWhenAll(permissionPair => permissionManagementService.UpdatePermission(permissionPair.Item1, permissionPair.Item2, cancellationToken));
    }
}