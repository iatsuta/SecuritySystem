using ExampleApp.Application;
using ExampleApp.Domain;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.AvailableSecurity;
using SecuritySystem.ExternalSystem;
using SecuritySystem.ExternalSystem.Management;
using SecuritySystem.Services;
using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public class GeneralPermissionTests : TestBase
{
    [Fact]
    public async Task AssignGeneralPermission_PermissionResolved()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";

        var buIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", cancellationToken);

        var testRole = ExampleRoles.BuManager;

        var testPermission = new ExampleTestPermission(testRole) { BusinessUnit = buIdentity };

        var principalIdentity = await this.AuthManager.For(principalName).SetRoleAsync(testPermission, cancellationToken);

        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var authenticationService = scope.ServiceProvider.GetRequiredService<TestRawUserAuthenticationService>();
        var availableSecurityRoleSource = scope.ServiceProvider.GetRequiredService<IAvailableSecurityRoleSource>();

        authenticationService.CurrentUserName = principalName;

        // Act
        var availableSecurityRoles = await availableSecurityRoleSource.GetAvailableSecurityRoles(true, cancellationToken);

        var managedPrincipal = await this.AuthManager.For(principalName).GetPrincipalAsync(cancellationToken);

        // Assert
        availableSecurityRoles.Should().BeEquivalentTo([testRole]);

        managedPrincipal.Header.Identity.Should().Be(principalIdentity);
        managedPrincipal.Header.Name.Should().Be(principalName);
        managedPrincipal.Header.IsVirtual.Should().Be(false);

        var managedPermission = managedPrincipal.Permissions.Should().ContainSingle().Subject;

        managedPermission.IsVirtual.Should().Be(false);
        managedPermission.Restrictions.Should().BeEquivalentTo(testPermission.Restrictions);
    }
}