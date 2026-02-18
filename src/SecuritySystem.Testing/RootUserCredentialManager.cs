using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Testing;

public class RootUserCredentialManager(
    AdministratorsRoleList administratorsRoleList,
    ITestingEvaluator<UserCredentialManager> baseEvaluator,
    ITestingUserAuthenticationService authenticationService,
    Tuple<UserCredential?> userCredential)
{
    private ITestingEvaluator<UserCredentialManager> ManagerEvaluator { get; } =
        baseEvaluator.Select(service => service.WithCredential(userCredential.Item1));

    public void LoginAs()
    {
        authenticationService.CustomUserCredential = userCredential.Item1;
    }

    public SecurityIdentity CreatePrincipal()
    {
        return this.CreatePrincipalAsync().GetAwaiter().GetResult();
    }

    public async Task<SecurityIdentity> CreatePrincipalAsync(CancellationToken cancellationToken = default)
    {
        return await this.ManagerEvaluator.EvaluateAsync(TestingScopeMode.Write, manager => manager.CreatePrincipalAsync(cancellationToken));
    }

    public SecurityIdentity SetAdminRole()
    {
        return this.SetAdminRoleAsync().GetAwaiter().GetResult();
    }

    public Task<SecurityIdentity> SetAdminRoleAsync(CancellationToken cancellationToken = default)
    {
        return this.SetRoleAsync(administratorsRoleList.Roles.Select(securityRole => new ManagedPermissionData { SecurityRole = securityRole }).ToArray(),
            cancellationToken);
    }

    public SecurityIdentity SetRole(params ManagedPermissionData[] permissions)
    {
        return this.SetRoleAsync(permissions).GetAwaiter().GetResult();
    }

    public async Task<SecurityIdentity> SetRoleAsync(ManagedPermissionData permission, CancellationToken cancellationToken = default)
    {
        return await this.SetRoleAsync([permission], cancellationToken);
    }

    public async Task<SecurityIdentity> SetRoleAsync(ManagedPermissionData[] permissions, CancellationToken cancellationToken = default)
    {
        await this.ClearRolesAsync(cancellationToken);

        return await this.AddRoleAsync(permissions, cancellationToken);
    }

    public SecurityIdentity AddRole(params ManagedPermissionData[] permissions) =>
        this.AddRoleAsync(permissions).GetAwaiter().GetResult();

    public async Task<SecurityIdentity> AddRoleAsync(ManagedPermissionData permission, CancellationToken cancellationToken = default) =>
        await this.AddRoleAsync([permission], cancellationToken);

    public async Task<SecurityIdentity> AddRoleAsync(ManagedPermissionData[] permissions, CancellationToken cancellationToken = default) =>
        await this.ManagerEvaluator.EvaluateAsync(TestingScopeMode.Write, async manager => await manager.AddUserRoleAsync(permissions, cancellationToken));

    public void ClearRoles()
    {
        this.ClearRolesAsync().GetAwaiter().GetResult();
    }

    public async Task ClearRolesAsync(CancellationToken cancellationToken = default)
    {
        await this.ManagerEvaluator.EvaluateAsync(TestingScopeMode.Write, async manager => await manager.RemovePermissionsAsync(cancellationToken));
    }

    public ManagedPrincipal GetPrincipal()
    {
        return this.GetPrincipalAsync().GetAwaiter().GetResult();
    }

    public async Task<ManagedPrincipal> GetPrincipalAsync(CancellationToken cancellationToken = default)
    {
        return await this.ManagerEvaluator.EvaluateAsync(TestingScopeMode.Read, async manager => await manager.GetPrincipalAsync(cancellationToken));
    }
}