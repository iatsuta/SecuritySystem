using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using GenericQueryable;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;

using SecuritySystem.Services;
using SecuritySystem.UserSource;


namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalManagementService<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType,
    TSecurityContextObjectIdent, TSecurityRoleIdent, TPermissionIdent, TSecurityContextTypeIdent>(
    ISecurityRepository<TSecurityRole> securityRoleRepository,
    IQueryableSource queryableSource,
    IVisualIdentityInfoSource visualIdentityInfoSource,
    IAvailablePrincipalSource<TPrincipal> availablePrincipalSource,
    ITypedPrincipalConverter<TPrincipal> typedPrincipalConverter,
    ISecurityValidator<PrincipalData> principalValidator,
    IUserQueryableSource<TPrincipal> userQueryableSource,

    ISecurityRoleSource securityRoleSource,

    GeneralPermissionBindingInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent>
        bindingInfo,

    IGenericRepository genericRepository,
    ISecurityRepository<TSecurityContextType> securityContextTypeRepository,

    ISecurityContextInfoSource securityContextInfoSource,

    IPrincipalDomainService<TPrincipal> principalDomainService,
    IUserSource<TPrincipal> principalUserSource,

    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo,
    IdentityInfo<TSecurityContextType, TSecurityContextTypeIdent> securityContextTypeIdentityInfo,
    IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentityInfo,
    ISecurityIdentityConverter<TPermissionIdent> permissionIdentConverter,

    ISecurityIdentityConverter<TSecurityContextTypeIdent> securityContextTypeIdentityConverter)
    : GeneralPrincipalSourceService<TPrincipal>(
            queryableSource,
            visualIdentityInfoSource,
            availablePrincipalSource,
            typedPrincipalConverter,
            userQueryableSource),
        IPrincipalManagementService

    where TPrincipal : class, new()
    where TPermission : class, new()
    where TSecurityRole : class
    where TSecurityRoleIdent : notnull
    where TPermissionIdent : notnull, new()
    where TPermissionRestriction : class, new()
    where TSecurityContextType : class
    where TSecurityContextObjectIdent : notnull
    where TSecurityContextTypeIdent : notnull
{
    public async Task<PrincipalData> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken)
    {
        var principal = await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);

        return new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(principal, []);
    }

    public async Task<PrincipalData> UpdatePrincipalNameAsync(
        UserCredential userCredential,
        string principalName,
        CancellationToken cancellationToken)
    {
        var principal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        this.NameAccessors.Setter(principal, principalName);

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
        var dbPermissions = await queryableSource.GetQueryable<TPermission>().Where(bindingInfo.Principal.Path.Select(p => p == dbPrincipal))
            .GenericToListAsync(cancellationToken);

        var permissionsData = await dbPermissions.SyncWhenAll(async dbPermission => await this.ToPermissionData(dbPermission, cancellationToken));

        return new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(dbPrincipal, permissionsData);
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>> ToPermissionData(TPermission dbPermission,
        CancellationToken cancellationToken)
    {
        var dbRestrictions = await queryableSource.GetQueryable<TPermissionRestriction>().Where(bindingInfo.Permission.Path.Select(p => p == dbPermission))
            .GenericToListAsync(cancellationToken);

        return new PermissionData<TPermission, TPermissionRestriction>(dbPermission, dbRestrictions.ToList());
    }

    public async Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(
        UserCredential userCredential,
        IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        var dbPrincipal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        var dbPermissions = await queryableSource.GetQueryable<TPermission>().Where(bindingInfo.Principal.Path.Select(p => p == dbPrincipal))
            .GenericToListAsync(cancellationToken);

        var permissionMergeResult = dbPermissions.GetMergeResult(typedPermissions, v => permissionIdentityInfo.Id.Getter(v),
            p => (object?)permissionIdentConverter.TryConvert(p.Identity) ?? new object());

        var newPermissions = await this.CreatePermissionsAsync(dbPrincipal, permissionMergeResult.AddingItems, cancellationToken);

        var updatedPermissions = await this.UpdatePermissionsAsync(permissionMergeResult.CombineItems, cancellationToken);

        var removingPermissions = await permissionMergeResult.RemovingItems.SyncWhenAll(async oldDbPermission =>
        {
            var result = await this.ToPermissionData(oldDbPermission, cancellationToken);

            await genericRepository.RemoveAsync(oldDbPermission, cancellationToken);

            return result;
        });

        await principalValidator.ValidateAsync(
            new PrincipalData<TPrincipal, TPermission, TPermissionRestriction>(dbPrincipal,
                updatedPermissions.Select(pair => pair.PermissonData).Concat(newPermissions).ToList()),
            cancellationToken);

        return new MergeResult<PermissionData, PermissionData>(
            newPermissions,
            updatedPermissions.Where(pair => pair.Updated).Select(pair => (PermissionData)pair.PermissonData).Select(v => (v, v)),
            removingPermissions);
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>[]> CreatePermissionsAsync(
        TPrincipal dbPrincipal,
        IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        return await typedPermissions.SyncWhenAll(typedPermission => this.CreatePermissionAsync(dbPrincipal, typedPermission, cancellationToken));
    }

    private async Task<PermissionData<TPermission, TPermissionRestriction>> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        TypedPermission typedPermission,
        CancellationToken cancellationToken)
    {
        if (typedPermission.Identity.IsDefault || typedPermission.IsVirtual)
        {
            throw new Exception("wrong typed permission");
        }

        var securityRole = securityRoleSource.GetSecurityRole(typedPermission.SecurityRole);

        var dbRole = await securityRoleRepository.GetObjectAsync(securityRole.Identity, cancellationToken);

        var newDbPermission = new TPermission();

        bindingInfo.Principal.Setter(newDbPermission, dbPrincipal);
        bindingInfo.SecurityRole.Setter(newDbPermission, dbRole);

        bindingInfo.PermissionPeriod?.Setter(newDbPermission, typedPermission.Period);
        bindingInfo.PermissionComment?.Setter(newDbPermission, typedPermission.Comment);

        await genericRepository.SaveAsync(newDbPermission, cancellationToken);

        var newPermissionRestrictions = await typedPermission.Restrictions.SyncWhenAll(async restrictionGroup =>
        {
            var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Identity;

            var dbSecurityContextType = await securityContextTypeRepository.GetObjectAsync(securityContextTypeIdentity, cancellationToken);

            var newPermissionRestrictions = await restrictionGroup.Value.Cast<TSecurityContextObjectIdent>().SyncWhenAll(async securityContextId =>
            {
                var newDbPermissionRestriction = new TPermissionRestriction();

                bindingInfo.Permission.Setter(newDbPermissionRestriction, newDbPermission);
                bindingInfo.SecurityContextObjectId.Setter(newDbPermissionRestriction, securityContextId);
                bindingInfo.SecurityContextType.Setter(newDbPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newDbPermissionRestriction, cancellationToken);

                return newDbPermissionRestriction;
            });

            return newPermissionRestrictions;
        });

        return new PermissionData<TPermission, TPermissionRestriction>(newDbPermission, newPermissionRestrictions.SelectMany().ToList());
    }

    private async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)[]> UpdatePermissionsAsync(
        IReadOnlyList<(TPermission, TypedPermission)> permissionPairs,
        CancellationToken cancellationToken)
    {
        return await permissionPairs.SyncWhenAll(permissionPair => this.UpdatePermission(permissionPair.Item1, permissionPair.Item2, cancellationToken));
    }

    private async Task<(PermissionData<TPermission, TPermissionRestriction> PermissonData, bool Updated)> UpdatePermission(TPermission dbPermission,
        TypedPermission typedPermission, CancellationToken cancellationToken)
    {
        var dbSecurityRoleId = bindingInfo.SecurityRole.Getter.Composite(securityRoleIdentityInfo.Id.Getter).Invoke(dbPermission);
        var dbSecurityRole = securityRoleSource.GetSecurityRole(new TypedSecurityIdentity<TSecurityRoleIdent>(dbSecurityRoleId));

        if (dbSecurityRole != typedPermission.SecurityRole)
        {
            throw new SecuritySystemException("Permission role can't be changed");
        }

        var dbRestrictions = await queryableSource.GetQueryable<TPermissionRestriction>()
            .Where(bindingInfo.Permission.Path.Select(p => p == dbPermission))
            .GenericToListAsync(cancellationToken);

        var restrictionMergeResult = dbRestrictions.GetMergeResult(
            typedPermission.Restrictions
                .ChangeKey(t => securityContextTypeIdentityConverter.Convert(securityContextInfoSource.GetSecurityContextInfo(t).Identity).Id)
                .SelectMany(pair => pair.Value.Cast<TSecurityContextObjectIdent>().Select(securityContextId => (pair.Key, securityContextId))),
            pr => (
                bindingInfo.SecurityContextType.Getter.Composite(securityContextTypeIdentityInfo.Id.Getter).Invoke(pr),
                bindingInfo.SecurityContextObjectId.Getter.Invoke(pr)),
            pair => pair);

        if (restrictionMergeResult.IsEmpty
            && (bindingInfo.PermissionComment == null || bindingInfo.PermissionComment.Getter(dbPermission) == typedPermission.Comment)
            && (bindingInfo.PermissionPeriod == null || bindingInfo.PermissionPeriod.Getter(dbPermission) == typedPermission.Period))
        {
            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1).ToList());

            return (permissionData, false);
        }
        else
        {
            bindingInfo.PermissionComment?.Setter.Invoke(dbPermission, typedPermission.Comment);
            bindingInfo.PermissionPeriod?.Setter.Invoke(dbPermission, typedPermission.Period);

            var newPermissionRestrictions = await restrictionMergeResult.AddingItems.SyncWhenAll(async restriction =>
            {
                var newPermissionRestriction = new TPermissionRestriction();

                var dbSecurityContextType =
                    await securityContextTypeRepository.GetObjectAsync(new TypedSecurityIdentity<TSecurityContextTypeIdent>(restriction.Key), cancellationToken);

                bindingInfo.SecurityContextObjectId.Setter(newPermissionRestriction, restriction.securityContextId);
                bindingInfo.SecurityContextType.Setter(newPermissionRestriction, dbSecurityContextType);

                await genericRepository.SaveAsync(newPermissionRestriction, cancellationToken);

                return newPermissionRestriction;
            });

            foreach (var dbRestriction in restrictionMergeResult.RemovingItems)
            {
                await genericRepository.RemoveAsync(dbRestriction, cancellationToken);
            }

            var permissionData = new PermissionData<TPermission, TPermissionRestriction>(dbPermission,
                restrictionMergeResult.CombineItems.Select(v => v.Item1).Concat(newPermissionRestrictions).ToList());

            return (permissionData, true);
        }
    }
}