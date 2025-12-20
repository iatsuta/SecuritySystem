using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Credential;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Testing;

public class RootUserCredentialManager(IServiceProvider rootServiceProvider, UserCredential? userCredential)
{
    private TestingEvaluator<AuthManager> ManagerEvaluator { get; } = new (rootServiceProvider, sp => ActivatorUtilities.CreateInstance<AuthManager>(sp, Tuple.Create(userCredential)));

    private ITestingUserAuthenticationService UserAuthenticationService =>
        rootServiceProvider.GetRequiredService<ITestingUserAuthenticationService>();

    private IEnumerable<SecurityRole> AdminRoles => rootServiceProvider.GetRequiredService<AdministratorsRoleList>().Roles;

    public void LoginAs()
    {
        this.UserAuthenticationService.SetUser(userCredential);
    }

    public SecurityIdentity CreatePrincipal()
    {
        return this.CreatePrincipalAsync().GetAwaiter().GetResult();
    }

    public async Task<SecurityIdentity> CreatePrincipalAsync(CancellationToken cancellationToken = default)
    {
        return await this.ManagerEvaluator.EvaluateAsync(manger => manger.CreatePrincipalAsync(cancellationToken));
    }

    public SecurityIdentity SetAdminRole()
    {
        return this.SetAdminRoleAsync().GetAwaiter().GetResult();
    }

    public Task<SecurityIdentity> SetAdminRoleAsync(CancellationToken cancellationToken = default)
    {
        return this.SetRoleAsync(this.AdminRoles.Select(v => (TestPermission)v).ToArray(), cancellationToken);
    }

    public SecurityIdentity SetRole(params TestPermission[] permissions)
    {
        return this.SetRoleAsync(permissions).GetAwaiter().GetResult();
    }

    public async Task<SecurityIdentity> SetRoleAsync(TestPermission[] permissions, CancellationToken cancellationToken = default)
    {
        await this.ClearRolesAsync(cancellationToken);

        return await this.AddRoleAsync(permissions, cancellationToken);
    }

    public async Task<SecurityIdentity> SetRoleAsync(TestPermission permission, CancellationToken cancellationToken = default)
    {
        return await this.AddRoleAsync([permission], cancellationToken);
    }

    public SecurityIdentity AddRole(params TestPermission[] permissions) =>
        this.AddRoleAsync(permissions).GetAwaiter().GetResult();

    public async Task<SecurityIdentity> AddRoleAsync(TestPermission[] permissions, CancellationToken cancellationToken = default) =>
        await this.ManagerEvaluator.EvaluateAsync(
            async manger => await manger.AddUserRoleAsync(permissions, cancellationToken));

    public async Task<SecurityIdentity> AddRoleAsync(TestPermission permission, CancellationToken cancellationToken = default) =>
        await this.AddRoleAsync([permission], cancellationToken);

    public void ClearRoles()
    {
        this.ClearRolesAsync().GetAwaiter().GetResult();
    }

    public async Task ClearRolesAsync(CancellationToken cancellationToken = default)
    {
        await this.ManagerEvaluator.EvaluateAsync(async manager => await manager.RemovePermissionsAsync(cancellationToken));
    }

    public ManagedPrincipal GetPrincipal()
    {
        return this.GetPrincipalAsync().GetAwaiter().GetResult();
    }

    public async Task<ManagedPrincipal> GetPrincipalAsync(CancellationToken cancellationToken = default)
    {
        return await this.ManagerEvaluator.EvaluateAsync(async manager => await manager.GetPrincipalAsync(cancellationToken));
    }
}