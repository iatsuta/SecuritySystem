using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public class UserCredentialManager(
    IRawUserAuthenticationService userAuthenticationService,
    IPrincipalManagementService principalManagementService,
    IRootPrincipalSourceService rootPrincipalSourceService,
    IUserCredentialNameResolver credentialNameResolver,
    IPrincipalDataSecurityIdentityExtractor securityIdentityExtractor,
    UserCredential? baseUserCredential = null)
{
    private readonly IPrincipalSourceService principalSourceService = rootPrincipalSourceService.ForPrincipal(principalManagementService.PrincipalType);

    private readonly UserCredential userCredential = baseUserCredential ?? userAuthenticationService.GetUserName();

    public UserCredentialManager WithCredential(UserCredential? newUserCredential)
    {
        return new UserCredentialManager(userAuthenticationService, principalManagementService, rootPrincipalSourceService, credentialNameResolver,
            securityIdentityExtractor, newUserCredential);
    }

    private string PrincipalName => credentialNameResolver.GetUserName(userCredential);

    public async Task<SecurityIdentity> CreatePrincipalAsync(CancellationToken cancellationToken = default)
    {
        var principalData = await principalManagementService.CreatePrincipalAsync(this.PrincipalName, [], cancellationToken);

        return securityIdentityExtractor.Extract(principalData);
    }

    public async Task<SecurityIdentity> AddUserRoleAsync(ManagedPermissionData[] testPermissions, CancellationToken cancellationToken = default)
    {
        var newPermissions = testPermissions.Select(testPermission => new ManagedPermission
        {
            Identity = SecurityIdentity.Default,
            IsVirtual = false,
            SecurityRole = testPermission.SecurityRole,
            Period = testPermission.Period,
            Comment = testPermission.Comment,
            Restrictions = testPermission.Restrictions
        });

        var existsPrincipal = await principalSourceService.TryGetPrincipalAsync(this.userCredential, cancellationToken);

        if (existsPrincipal == null)
        {
            var newPrincipalData = await principalManagementService.CreatePrincipalAsync(this.PrincipalName, newPermissions, cancellationToken);

            return securityIdentityExtractor.Extract(newPrincipalData);
        }
        else
        {
            var updatedPrincipal = existsPrincipal with { Permissions = existsPrincipal.Permissions.Concat(newPermissions).ToList() };

            await principalManagementService.UpdatePermissionsAsync(
                updatedPrincipal.Header.Identity,
                updatedPrincipal.Permissions,
                cancellationToken);

            return updatedPrincipal.Header.Identity;
        }
    }

    public async Task RemovePermissionsAsync(CancellationToken cancellationToken = default)
    {
        var principal = await principalSourceService.TryGetPrincipalAsync(this.userCredential, cancellationToken);

        if (principal is { Header.IsVirtual: false })
        {
            await principalManagementService.RemovePrincipalAsync(principal.Header.Identity, true, cancellationToken);
        }
    }

    public async Task<ManagedPrincipal> GetPrincipalAsync(CancellationToken cancellationToken = default)
    {
        return await principalSourceService.GetPrincipalAsync(this.userCredential, cancellationToken);
    }
}