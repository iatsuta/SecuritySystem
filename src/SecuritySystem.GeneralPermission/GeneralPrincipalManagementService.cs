using CommonFramework;
using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace SecuritySystem.GeneralPermission;

public class GeneralPrincipalManagementService<TPrincipal, TPermission, TSecurityRole, TSecurityContextType>(
    IQueryableSource queryableSource,
    ISecurityRoleSource securityRoleSource,
    ISecurityContextInfoSource securityContextInfoSource,
    IAvailablePermissionSource<TPermission> availablePermissionSource,
    IPrincipalDomainService<TPrincipal> principalDomainService,
    IUserSource<TPrincipal> principalUserSource)
    : GeneralPrincipalSourceService(
		    queryableSource,
      securityRoleSource,
      securityContextInfoSource,
      availablePermissionSource),
      IPrincipalManagementService

{
    public async Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default)
    {
        return await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);
    }

    public async Task<object> UpdatePrincipalNameAsync(
        UserCredential userCredential,
        string principalName,
        CancellationToken cancellationToken)
    {
        var principal = await principalResolver.Resolve(userCredential, cancellationToken);

        principal.Name = principalName;

        await principalRepository.SaveAsync(principal, cancellationToken);

        return principal;
    }

    public async Task<object> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default)
    {
        var principal = await principalResolver.Resolve(userCredential, cancellationToken);

        await principalDomainService.RemoveAsync(principal, force, cancellationToken);

        return principal;
    }

    public async Task<MergeResult<object, object>> UpdatePermissionsAsync(
        TSecurityContextObjectIdent principalId,
        IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken = default)
    {
        var dbPrincipal = await principalRepository.LoadAsync(principalId, cancellationToken);

        var permissionMergeResult = dbPrincipal.Permissions.GetMergeResult(typedPermissions, p => p.Id, p => p.Id == TSecurityContextObjectIdent.Empty ? TSecurityContextObjectIdent.NewTSecurityContextIdent() : p.Id);

        var newPermissions = await this.CreatePermissionsAsync(dbPrincipal, permissionMergeResult.AddingItems, cancellationToken);

        var updatedPermissions = await this.UpdatePermissionsAsync(permissionMergeResult.CombineItems, cancellationToken);

        foreach (var oldDbPermission in permissionMergeResult.RemovingItems)
        {
            dbPrincipal.RemoveDetail(oldDbPermission);

            await permissionRepository.RemoveAsync(oldDbPermission, cancellationToken);
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
        CancellationToken cancellationToken = default)
    {
        return await typedPermissions.SyncWhenAll(
                   typedPermission => this.CreatePermissionAsync(dbPrincipal, typedPermission, cancellationToken));
    }

    private async Task<TPermission> CreatePermissionAsync(
        TPrincipal dbPrincipal,
        TypedPermission typedPermission,
        CancellationToken cancellationToken = default)
    {
        if (typedPermission.Id != TSecurityContextObjectIdent.Empty || typedPermission.IsVirtual)
        {
            throw new Exception("wrong typed permission");
        }

        var securityRole = securityRoleSource.GetSecurityRole(typedPermission.SecurityRole);

        var dbRole = await securityRoleRepository.LoadAsync(securityRole.Id, cancellationToken);

        var newDbPermission = new TPermission(dbPrincipal)
                              {
                                  Comment = typedPermission.Comment, Period = typedPermission.GetPeriod(), Role = dbRole
                              };

        foreach (var restrictionGroup in typedPermission.Restrictions)
        {
            var securityContextTypeId = securityContextInfoSource.GetSecurityContextInfo(restrictionGroup.Key).Id;

            foreach (TSecurityContextObjectIdent securityContextId in restrictionGroup.Value)
            {
                _ = new PermissionRestriction(newDbPermission)
                    {
                        SecurityContextId = securityContextId,
                        TSecurityContextType = await securityContextTypeRepository.LoadAsync(
                                                  securityContextTypeId,
                                                  cancellationToken)
                    };
            }
        }

        await permissionRepository.SaveAsync(newDbPermission, cancellationToken);

        return newDbPermission;
    }

    private async Task<IReadOnlyList<(TPermission, TypedPermission)>> UpdatePermissionsAsync(
        IReadOnlyList<(TPermission, TypedPermission)> permissionPairs,
        CancellationToken cancellationToken = default)
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
        if (securityRoleSource.GetSecurityRole(dbPermission.Role.Id) != typedPermission.SecurityRole)
        {
            throw new SecuritySystemException("TPermission role can't be changed");
        }

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
