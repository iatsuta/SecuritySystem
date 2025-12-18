using ExampleApp.Application;
using ExampleApp.Domain.Auth.General;
using ExampleApp.IntegrationTests.Services;

using GenericQueryable;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.GeneralPermission;
using SecuritySystem.Services;
using SecuritySystem.UserSource;

namespace ExampleApp.IntegrationTests;

public class GeneralPermissionTests : TestBase
{
    [Fact]
    public async Task CratInvokeExpandWithParents_ForRootBu_DataCorrected()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";

        var principalId = await this.CreateTestPrincipal(principalName, ExampleRoles.TestManager, cancellationToken);

        await using var scope = this.RootServiceProvider.CreateAsyncScope();
        var authenticationService = scope.ServiceProvider.GetRequiredService<TestRawUserAuthenticationService>();
        authenticationService.CurrentUserName = principalName;

        // Act
        var currentUserSource = scope.ServiceProvider.GetRequiredService<ICurrentUserSource<Principal>>();

        // Assert
        principalId.Should().Be(currentUserSource.CurrentUser.Id);
    }

    private async Task<Guid> CreateTestPrincipal(string principalName, SecuritySystem.SecurityRole securityRole, CancellationToken cancellationToken)
    {
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var serviceProvider = scope.ServiceProvider;

        var securityRoleRepository = serviceProvider.GetRequiredService<IRepository<SecurityRole>>();
        var permissionRepository = serviceProvider.GetRequiredService<IRepository<Permission>>();

        var dbSecurityRole = await securityRoleRepository.GetQueryable().GenericSingleAsync(sr => sr.Name == securityRole.Name, cancellationToken);

        var principalDomainService = serviceProvider.GetRequiredService<IPrincipalDomainService<Principal>>();

        var principalRootValidator = serviceProvider.GetRequiredService<ISecurityValidator<PrincipalData>>();

        var principal = await principalDomainService.GetOrCreateAsync(principalName, cancellationToken);

        var permission = new Permission { Principal = principal, SecurityRole = dbSecurityRole };

        await permissionRepository.SaveAsync(permission, cancellationToken);

        await principalRootValidator.ValidateAsync(new PrincipalData<Principal, Permission, PermissionRestriction>(principal,
            [new PermissionData<Permission, PermissionRestriction>(permission, [])]), cancellationToken);

        return principal.Id;
    }
}