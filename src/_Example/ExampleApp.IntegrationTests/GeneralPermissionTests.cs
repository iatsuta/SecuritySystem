using ExampleApp.Application;
using ExampleApp.Domain.Auth.General;
using ExampleApp.IntegrationTests.Services;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;
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

        var principalIdentity = await this.AuthManager.For(principalName).SetRoleAsync(ExampleRoles.TestManager, cancellationToken);

        await using var scope = this.RootServiceProvider.CreateAsyncScope();
        var authenticationService = scope.ServiceProvider.GetRequiredService<TestRawUserAuthenticationService>();
        authenticationService.CurrentUserName = principalName;

        // Act
        var currentUserSource = scope.ServiceProvider.GetRequiredService<ICurrentUserSource<Principal>>();

        // Assert
        principalIdentity.Should().Be(TypedSecurityIdentity.Create(currentUserSource.CurrentUser.Id));
    }
}