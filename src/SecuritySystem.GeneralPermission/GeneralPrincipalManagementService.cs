using CommonFramework;
using CommonFramework.DependencyInjection;
using CommonFramework.GenericRepository;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

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
    IGeneralPermissionBindingInfoSource generalBindingInfoSource,
    IGeneralPermissionRestrictionBindingInfoSource restrictionBindingInfoSource)
    : IPrincipalManagementService
{
    private readonly Lazy<IPrincipalManagementService> lazyInnerService = new(() =>
    {
        var bindingInfo = bindingInfoList.Single(bi => !bi.IsReadonly);

        var generalBindingInfo = generalBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var restrictionBindingInfo = restrictionBindingInfoSource.GetForPermission(bindingInfo.PermissionType);

        var principalVisualIdentityInfo = visualIdentityInfoSource.GetVisualIdentityInfo(bindingInfo.PrincipalType);

        var innerServiceType = typeof(GeneralPrincipalManagementService<,,,,,>)
            .MakeGenericType(
                bindingInfo.PrincipalType,
                bindingInfo.PermissionType,
                generalBindingInfo.SecurityRoleType,
                restrictionBindingInfo.PermissionRestrictionType,
                restrictionBindingInfo.SecurityContextTypeType,
                restrictionBindingInfo.SecurityContextObjectIdentType);

        return serviceProxyFactory.Create<IPrincipalManagementService>(
            innerServiceType,
            bindingInfo,
            generalBindingInfo,
            restrictionBindingInfo,
            principalVisualIdentityInfo);
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

public class GeneralPrincipalManagementService<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent>(
    PermissionBindingInfo<TPermission, TPrincipal> bindingInfo,
    GeneralPermissionBindingInfo<TPermission, TSecurityRole> generalBindingInfo,
    GeneralPermissionRestrictionBindingInfo<TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TPermission> restrictionBindingInfo,
    ISecurityRepository<TSecurityRole> securityRoleRepository,
    IQueryableSource queryableSource,
    [FromKeyedServices("Root")] IPrincipalValidator<TPrincipal, TPermission, TPermissionRestriction> principalValidator,
    ISecurityRoleSource securityRoleSource,
    IGenericRepository genericRepository,
    ISecurityRepository<TSecurityContextType> securityContextTypeRepository,
    ISecurityContextInfoSource securityContextInfoSource,
    IPrincipalDomainService<TPrincipal> principalDomainService,
    IUserSource<TPrincipal> principalUserSource,
    ISecurityIdentityExtractorFactory securityIdentityExtractorFactory,
    IPermissionLoader<TPrincipal, TPermission> permissionLoader,
    IPermissionRestrictionLoader<TPermission, TPermissionRestriction> permissionRestrictionLoader,
    VisualIdentityInfo<TPrincipal> principalVisualIdentityInfo)
    : IPrincipalManagementService

    where TPrincipal : class, new()
    where TPermission : class, new()
    where TSecurityRole : class
    where TPermissionRestriction : class, new()
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
{
    private readonly ISecurityIdentityExtractor<TPermission> permissionIdentityExtractor = securityIdentityExtractorFactory.Create<TPermission>();

    private readonly ISecurityIdentityExtractor<TSecurityRole> securityRoleIdentityExtractor = securityIdentityExtractorFactory.Create<TSecurityRole>();

    private readonly ISecurityIdentityExtractor<TSecurityContextType> securityContextTypeIdentityExtractor =
        securityIdentityExtractorFactory.Create<TSecurityContextType>();

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
            p => p.Identity.IsDefault ? new object() : permissionIdentityExtractor.Converter.TryConvert(p.Identity));

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
        return await typedPermissions.SyncWhenAll(managedPermission => this.CreatePermissionAsync(dbPrincipal, managedPermission, cancellationToken));
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        ManagedPermission managedPermission,
        CancellationToken cancellationToken)
    {
        if (!managedPermission.Identity.IsDefault || managedPermission.IsVirtual)
        {
            throw new SecuritySystemException("wrong typed permission");
        }

        var securityRole = securityRoleSource.GetSecurityRole(managedPermission.SecurityRole);

        var dbRole = await securityRoleRepository.GetObjectAsync(securityRole.Identity, cancellationToken);

        var newDbPermission = new TPermission();

        bindingInfo.Principal.Setter(newDbPermission, dbPrincipal);
        generalBindingInfo.SecurityRole.Setter(newDbPermission, dbRole);

        bindingInfo.PermissionStartDate?.Setter(newDbPermission, managedPermission.Period.StartDate);
        bindingInfo.PermissionEndDate?.Setter(newDbPermission, managedPermission.Period.EndDate);
        bindingInfo.PermissionComment?.Setter(newDbPermission, managedPermission.Comment);

        await genericRepository.SaveAsync(newDbPermission, cancellationToken);

        var newPermissionRestrictions = await managedPermission.Restrictions.SyncWhenAll(async restrictionGroup =>
        {
            var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Identity;

            var dbSecurityContextType = await securityContextTypeRepository.GetObjectAsync(securityContextTypeIdentity, cancellationToken);

            var newPermissionRestrictions = await restrictionGroup.Value.Cast<TSecurityContextObjectIdent>().SyncWhenAll(async securityContextId =>
            {
                var newDbPermissionRestriction = new TPermissionRestriction();

                restrictionBindingInfo.Permission.Setter(newDbPermissionRestriction, newDbPermission);
                restrictionBindingInfo.SecurityContextObjectId.Setter(newDbPermissionRestriction, securityContextId);
                restrictionBindingInfo.SecurityContextType.Setter(newDbPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newDbPermissionRestriction, cancellationToken);

                return newDbPermissionRestriction;
            });

            return newPermissionRestrictions;
        });

        return new PermissionData<TPermission, TPermissionRestriction>(newDbPermission, newPermissionRestrictions.SelectMany());
    }

    private async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)[]> UpdatePermissionsAsync(
        IReadOnlyList<(TPermission, ManagedPermission)> permissionPairs,
        CancellationToken cancellationToken)
    {
        return await permissionPairs.SyncWhenAll(permissionPair => this.UpdatePermission(permissionPair.Item1, permissionPair.Item2, cancellationToken));
    }

    private async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(TPermission dbPermission,
        ManagedPermission managedPermission, CancellationToken cancellationToken)
    {
        if (managedPermission.Identity.IsDefault || managedPermission.IsVirtual)
        {
            throw new SecuritySystemException("wrong typed permission");
        }

        var securityRole = generalBindingInfo
            .SecurityRole
            .Getter(dbPermission)
            .Pipe(securityRoleIdentityExtractor.Extract)
            .Pipe(securityRoleSource.GetSecurityRole);

        if (securityRole != managedPermission.SecurityRole)
        {
            throw new SecuritySystemException("Permission role can't be changed");
        }

        var dbRestrictions = await queryableSource.GetQueryable<TPermissionRestriction>()
            .Where(restrictionBindingInfo.Permission.Path.Select(p => p == dbPermission))
            .GenericToListAsync(cancellationToken);

        var restrictionMergeResult = dbRestrictions.GetMergeResult(
            managedPermission.Restrictions
                .ChangeKey(t => securityContextInfoSource.GetSecurityContextInfo(t).Identity)
                .SelectMany(pair => pair.Value.Cast<TSecurityContextObjectIdent>().Select(securityContextId => (pair.Key, securityContextId))),
            pr => (
                securityContextTypeIdentityExtractor.Extract(restrictionBindingInfo.SecurityContextType.Getter(pr)),
                restrictionBindingInfo.SecurityContextObjectId.Getter(pr)),
            pair => pair);

        if (restrictionMergeResult.IsEmpty
            && (bindingInfo.PermissionComment == null || bindingInfo.PermissionComment.Getter(dbPermission) == managedPermission.Comment)
            && (bindingInfo.PermissionStartDate == null || bindingInfo.PermissionStartDate.Getter(dbPermission) == managedPermission.Period.StartDate)
            && (bindingInfo.PermissionEndDate == null || bindingInfo.PermissionEndDate.Getter(dbPermission) == managedPermission.Period.EndDate))
        {
            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1));

            return (permissionData, false);
        }
        else
        {
            bindingInfo.PermissionComment?.Setter.Invoke(dbPermission, managedPermission.Comment);
            bindingInfo.PermissionStartDate?.Setter.Invoke(dbPermission, managedPermission.Period.StartDate);
            bindingInfo.PermissionEndDate?.Setter.Invoke(dbPermission, managedPermission.Period.EndDate);

            var newPermissionRestrictions = await restrictionMergeResult.AddingItems.SyncWhenAll(async restriction =>
            {
                var newPermissionRestriction = new TPermissionRestriction();

                var dbSecurityContextType =
                    await securityContextTypeRepository.GetObjectAsync(restriction.Key, cancellationToken);

                restrictionBindingInfo.Permission.Setter(newPermissionRestriction, dbPermission);
                restrictionBindingInfo.SecurityContextObjectId.Setter(newPermissionRestriction, restriction.securityContextId);
                restrictionBindingInfo.SecurityContextType.Setter(newPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newPermissionRestriction, cancellationToken);

                return newPermissionRestriction;
            });

            foreach (var dbRestriction in restrictionMergeResult.RemovingItems)
            {
                await genericRepository.RemoveAsync(dbRestriction, cancellationToken);
            }

            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1).Concat(newPermissionRestrictions));

            await genericRepository.SaveAsync(dbPermission, cancellationToken);

            return (permissionData, true);
        }
    }
}