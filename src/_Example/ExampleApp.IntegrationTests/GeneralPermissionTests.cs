using ExampleApp.Application;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.AvailableSecurity;
using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public class GeneralPermissionTests : TestBase
{
    [Fact]
    public async Task CratInvokeExpandWithParents_ForRootBu_DataCorrected()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";

        var testRole = ExampleRoles.TestManager;

        await this.AuthManager.For(principalName).SetRoleAsync(ExampleRoles.TestManager, cancellationToken);

        await using var scope = this.RootServiceProvider.CreateAsyncScope();
        var availableSecurityRoleSource = scope.ServiceProvider.GetRequiredService<IAvailableSecurityRoleSource>();

        var authenticationService = scope.ServiceProvider.GetRequiredService<TestRawUserAuthenticationService>();
        authenticationService.CurrentUserName = principalName;

        // Act
        var availableSecurityRoles = await availableSecurityRoleSource.GetAvailableSecurityRoles(true, cancellationToken);

        // Assert
        availableSecurityRoles.Should().BeEquivalentTo([testRole]);
    }
}