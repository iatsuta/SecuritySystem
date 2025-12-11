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
	IFormatProviderSource formatProviderSource,

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
	where TPermissionIdent : IParsable<TPermissionIdent>, new()
	where TPermissionRestriction : class, new()
	where TSecurityContextType : class
	where TSecurityContextObjectIdent : notnull
	where TSecurityContextTypeIdent : notnull
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

		var dbPermissions = await queryableSource.GetQueryable<TPermission>().Where(bindingInfo.Principal.Path.Select(p => p == dbPrincipal))
			.GenericToListAsync(cancellationToken);

		var permissionMergeResult = dbPermissions.GetMergeResult(typedPermissions, permissionIdentityInfo.Id.Getter,
			p => TPermissionIdent.TryParse(p.Id, formatProviderSource.FormatProvider, out var id) ? id : new TPermissionIdent());

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
		return await typedPermissions.SyncWhenAll(typedPermission => this.CreatePermissionAsync(dbPrincipal, typedPermission, cancellationToken));
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

		bindingInfo.Principal.Setter(newDbPermission, dbPrincipal);
		bindingInfo.SecurityRole.Setter(newDbPermission, dbRole);

		bindingInfo.Period?.Setter(newDbPermission, typedPermission.Period);
		bindingInfo.Comment?.Setter(newDbPermission, typedPermission.Comment);

		foreach (var restrictionGroup in typedPermission.Restrictions)
		{
			var securityContextTypeIdentity = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Identity;

			var dbSecurityContextType = await securityContextTypeRepository.GetObjectAsync(securityContextTypeIdentity, cancellationToken);

			foreach (TSecurityContextObjectIdent securityContextId in restrictionGroup.Value)
			{
				var dbPermissionRestriction = new TPermissionRestriction();

				bindingInfo.Permission.Setter(dbPermissionRestriction, newDbPermission);
				bindingInfo.SecurityContextObjectId.Setter(dbPermissionRestriction, securityContextId);
				bindingInfo.SecurityContextType.Setter(dbPermissionRestriction, dbSecurityContextType);
			}
		}

		await genericRepository.SaveAsync(newDbPermission, cancellationToken);

		return newDbPermission;
	}

	private async Task<IReadOnlyList<(TPermission, TypedPermission)>> UpdatePermissionsAsync(
		IReadOnlyList<(TPermission, TypedPermission)> permissionPairs,
		CancellationToken cancellationToken)
	{
		var preResult = await permissionPairs.SyncWhenAll(async permissionPair => new
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
		var dbSecurityRoleId = bindingInfo.SecurityRole.Getter.Composite(securityRoleIdentityInfo.Id.Getter).Invoke(dbPermission);
		var dbSecurityRole = securityRoleSource.GetSecurityRole(new SecurityIdentity<TSecurityRoleIdent>(dbSecurityRoleId));

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
		    && (bindingInfo.Comment == null || bindingInfo.Comment.Getter(dbPermission) == typedPermission.Comment)
		    && (bindingInfo.Period == null || bindingInfo.Period.Getter(dbPermission) == typedPermission.Period))
		{
			return false;
		}

		bindingInfo.Comment?.Setter.Invoke(dbPermission, typedPermission.Comment);
		bindingInfo.Period?.Setter.Invoke(dbPermission, typedPermission.Period);

		foreach (var restriction in restrictionMergeResult.AddingItems)
		{
			var newPermissionRestriction = new TPermissionRestriction();

			var dbSecurityContextType =
				await securityContextTypeRepository.GetObjectAsync(new SecurityIdentity<TSecurityContextTypeIdent>(restriction.Key), cancellationToken);

			bindingInfo.SecurityContextObjectId.Setter(newPermissionRestriction, restriction.securityContextId);
			bindingInfo.SecurityContextType.Setter(newPermissionRestriction, dbSecurityContextType);

			await genericRepository.SaveAsync(newPermissionRestriction, cancellationToken);
		}

		foreach (var dbRestriction in restrictionMergeResult.RemovingItems)
		{
			await genericRepository.RemoveAsync(dbRestriction, cancellationToken);
		}

		return true;
	}
}