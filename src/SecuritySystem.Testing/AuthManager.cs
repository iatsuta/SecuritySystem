using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;

namespace SecuritySystem.Testing;

public class AuthManager(
    Tuple<UserCredential?> baseUserCredential,
    IRawUserAuthenticationService userAuthenticationService,
    IPrincipalManagementService principalManagementService,
    IRootPrincipalSourceService rootPrincipalSourceService,
    IUserCredentialNameResolver credentialNameResolver,
    IPrincipalDataSecurityIdentityExtractor securityIdentityExtractor)
{
    private readonly IPrincipalSourceService principalSourceService = rootPrincipalSourceService.ForPrincipal(principalManagementService.PrincipalType);

    private readonly UserCredential userCredential = baseUserCredential.Item1 ?? userAuthenticationService.GetUserName();

    private string PrincipalName => credentialNameResolver.GetUserName(userCredential);

    public async Task<SecurityIdentity> CreatePrincipalAsync(CancellationToken cancellationToken = default)
    {
        var principalData = await principalManagementService.CreatePrincipalAsync(this.PrincipalName, cancellationToken);

        return securityIdentityExtractor.Extract(principalData);
    }

    public async Task<SecurityIdentity> AddUserRoleAsync(TestPermission[] testPermissions, CancellationToken cancellationToken = default)
    {
        var existsPrincipal = await principalSourceService.TryGetPrincipalAsync(this.userCredential, cancellationToken);

        var preUpdatePrincipal = existsPrincipal ?? await this.RawCreatePrincipalAsync(cancellationToken);

        var newPermissions = testPermissions.Select(testPermission => new ManagedPermission(
            SecurityIdentity.Default,
            false,
            testPermission.SecurityRole,
            testPermission.Period,
            nameof(TestPermission),
            testPermission.Restrictions));

        var updatedPrincipal = preUpdatePrincipal with { Permissions = preUpdatePrincipal.Permissions.Concat(newPermissions).ToList() };

        await principalManagementService.UpdatePermissionsAsync(
            updatedPrincipal.Header.Identity,
            updatedPrincipal.Permissions,
            cancellationToken);

        return updatedPrincipal.Header.Identity;
    }

    private async Task<ManagedPrincipal> RawCreatePrincipalAsync(CancellationToken cancellationToken)
    {
        var newPrincipalData = await principalManagementService.CreatePrincipalAsync(this.PrincipalName, cancellationToken);

        return new ManagedPrincipal(new ManagedPrincipalHeader(securityIdentityExtractor.Extract(newPrincipalData), this.PrincipalName, false), []);
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