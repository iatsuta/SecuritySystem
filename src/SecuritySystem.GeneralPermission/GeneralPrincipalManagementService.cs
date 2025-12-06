using CommonFramework;
using CommonFramework.GenericRepository;
using CommonFramework.IdentitySource;
using CommonFramework.VisualIdentitySource;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

using GenericQueryable;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalManagementService<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent, TSecurityRoleIdent, TPermissionIdent>(
    ISecurityRepository<TSecurityRole> securityRoleRepository,
	IQueryableSource queryableSource,
	IVisualIdentityInfoSource visualIdentityInfoSource,
	IAvailablePrincipalSource<TPrincipal> availablePrincipalSource,
	ITypedPrincipalConverter<TPrincipal> typedPrincipalConverter,
	IUserQueryableSource<TPrincipal> userQueryableSource,

    GeneralPermissionSystemInfo<TPrincipal, TPermission, TSecurityRole, TPermissionRestriction, TSecurityContextType, TSecurityContextObjectIdent> generalPermissionSystemInfo,

    IdentityInfo<TPermission, TPermissionIdent> permissionIdentityInfo,

	IGenericRepository genericRepository,
    ISecurityRepository<TSecurityContextType> securityContextTypeRepository,

    ISecurityContextInfoSource securityContextInfoSource,

    IPrincipalDomainService<TPrincipal> principalDomainService,
	IUserSource<TPrincipal> principalUserSource,

	ISecurityRoleSource securityRoleSource,
	IdentityInfo<TSecurityRole, TSecurityRoleIdent> securityRoleIdentityInfo,
	ISecurityIdentityConverter<TSecurityRoleIdent> securityRoleIdentityConverter,
    ISecurityIdentityConverter<TSecurityContextObjectIdent> securityContextObjectIdentityConverter)
    : GeneralPrincipalSourceService<TPrincipal>(
		    queryableSource,
		    visualIdentityInfoSource,
		    availablePrincipalSource,
            typedPrincipalConverter,
		    userQueryableSource),
	    IPrincipalManagementService

	where TPrincipal: class, new()
	where TPermission : class, new()
	where TSecurityRole : class
	where TSecurityRoleIdent : notnull
	where TPermissionIdent : notnull, IParsable<TPermissionIdent>, new()
	where TPermissionRestriction : class, new()
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
{
    public async Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken)
    {
        return await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);
    }

    public async Task<object> UpdatePrincipalNameAsync(
		UserCredential userCredential,
		string principalName,
        CancellationToken cancellationToken)
    {
        var principal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        this.NameAccessors.Setter(principal, principalName);

        await genericRepository.SaveAsync(principal, cancellationToken);

        return principal;
    }

    public async Task<object> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken)
    {
        var principal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        await principalDomainService.RemoveAsync(principal, force, cancellationToken);

        return principal;
    }

    public async Task<MergeResult<object, object>> UpdatePermissionsAsync(
		UserCredential userCredential,
		IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        var dbPrincipal = await principalUserSource.GetUserAsync(userCredential, cancellationToken);

        var dbPermissions = await queryableSource.GetQueryable<TPermission>().Where(generalPermissionSystemInfo.ToPrincipal.Path.Select(p => p == dbPrincipal))
	        .GenericToListAsync(cancellationToken);

        var permissionMergeResult = dbPermissions.GetMergeResult(typedPermissions, permissionIdentityInfo.Id.Getter,
	        p => TPermissionIdent.TryParse(p.Id, null, out var id) ? id : new TPermissionIdent());

        var newPermissions = await this.CreatePermissionsAsync(dbPrincipal, permissionMergeResult.AddingItems, cancellationToken);

        var updatedPermissions = await this.UpdatePermissionsAsync(permissionMergeResult.CombineItems, cancellationToken);

        foreach (var oldDbPermission in permissionMergeResult.RemovingItems)
        {
	        await genericRepository.RemoveAsync(oldDbPermission, cancellationToken);
        }

        await principalDomainService.ValidateAsync(dbPrincipal, cancellationToken);

        return new MergeResult<object, object>(
            newPermissions,
            updatedPermissions.Select(pair => (object)pair.Item1).Select(v => (v, v)),
            permissionMergeResult.RemovingItems);
    }

    private async Task<IReadOnlyList<TPermission>> CreatePermissionsAsync(
        TPrincipal dbPrincipal,
        IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken)
    {
        return await typedPermissions.SyncWhenAll(
                   typedPermission => this.CreatePermissionAsync(dbPrincipal, typedPermission, cancellationToken));
    }

    private async Task<TPermission> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        TypedPermission typedPermission,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(typedPermission.Id) || typedPermission.IsVirtual)
        {
            throw new Exception("wrong typed permission");
        }

        var securityRole = securityRoleSource.GetSecurityRole(typedPermission.SecurityRole);

        var dbRole = await securityRoleRepository.GetObjectAsync(securityRole.Identity, cancellationToken);

        var newDbPermission = new TPermission();

        generalPermissionSystemInfo.ToPrincipal.Setter(newDbPermission, dbPrincipal);
		generalPermissionSystemInfo.ToSecurityRole.Setter(newDbPermission, dbRole);

		generalPermissionSystemInfo.Period?.Setter(newDbPermission, (typedPermission.StartDate, typedPermission.EndDate));
		generalPermissionSystemInfo.Comment?.Setter(newDbPermission, typedPermission.Comment);

        foreach (var restrictionGroup in typedPermission.Restrictions)
        {
	        var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Identity;

	        var dbSecurityContextType = await securityContextTypeRepository.GetObjectAsync(securityContextTypeIdentity, cancellationToken);

            foreach (TSecurityContextObjectIdent securityContextId in restrictionGroup.Value)
            {
	            var dbPermissionRestriction = new TPermissionRestriction();

	            generalPermissionSystemInfo.ToPermission.Setter(dbPermissionRestriction, newDbPermission);
	            generalPermissionSystemInfo.ToSecurityContextObjectId.Setter(dbPermissionRestriction, securityContextId);
	            generalPermissionSystemInfo.ToSecurityContextType.Setter(dbPermissionRestriction, dbSecurityContextType);
            }
        }

        await permissionRepository.SaveAsync(newDbPermission, cancellationToken);

        return newDbPermission;
    }

    private async Task<IReadOnlyList<(TPermission, TypedPermission)>> UpdatePermissionsAsync(
        IReadOnlyList<(TPermission, TypedPermission)> permissionPairs,
        CancellationToken cancellationToken)
    {
        var preResult = await permissionPairs.SyncWhenAll(
                            async permissionPair => new
                                                    {
                                                        permissionPair,
                                                        Updated = await this.UpdatePermission(
                                                                      permissionPair.Item1,
                                                                      permissionPair.Item2,
                                                                      cancellationToken)
                                                    });

        return preResult
               .Where(pair => pair.Updated)
               .Select(pair => pair.permissionPair)
               .ToList();
    }

    private async Task<bool> UpdatePermission(TPermission dbPermission, TypedPermission typedPermission, CancellationToken cancellationToken)
    {
	    var dbSecurityRoleId = permissionToSecurityRoleInfo.ToSecurityRole.Getter.Composite(securityRoleIdentityInfo.Id.Getter).Invoke(dbPermission);
	    var dbSecurityRole = securityRoleSource.GetSecurityRole(new SecurityIdentity<TSecurityRoleIdent>(dbSecurityRoleId));

	    if (dbSecurityRole != typedPermission.SecurityRole)
	    {
		    throw new SecuritySystemException("TPermission role can't be changed");
	    }

        var dbRestrictions =

	    var restrictionMergeResult = dbPermission.Restrictions.GetMergeResult(
            typedPermission.Restrictions.ChangeKey(t => securityContextInfoSource.GetSecurityContextInfo(t).Id)
                           .SelectMany(pair => pair.Value.Cast<TSecurityContextObjectIdent>().Select(securityContextId => (pair.Key, securityContextId))),
            r => (r.SecurityContextType.Id, r.SecurityContextId),
            pair => pair);

        if (restrictionMergeResult.IsEmpty
            && dbPermission.Comment == typedPermission.Comment
            && dbPermission.Period ==  typedPermission.GetPeriod())
        {
            return false;
        }

        dbPermission.Comment = typedPermission.Comment;
        dbPermission.Period = typedPermission.GetPeriod();

        foreach (var restriction in restrictionMergeResult.AddingItems)
        {
            _ = new PermissionRestriction(dbPermission)
                {
                    SecurityContextId = restriction.securityContextId,
                    TSecurityContextType = await securityContextTypeRepository.LoadAsync(restriction.Key, cancellationToken)
                };
        }

        foreach (var dbRestriction in restrictionMergeResult.RemovingItems)
        {
            dbPermission.RemoveDetail(dbRestriction);
        }

        return true;
    }
}
