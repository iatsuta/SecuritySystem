using ExampleApp.Application;
using ExampleApp.Domain;

using SecuritySystem;
using SecuritySystem.Validation;

namespace ExampleApp.IntegrationTests;

public class DuplicatePermissionValidationTests : TestBase
{
    [Fact]
    public async Task AddRoleAsync_WhenDuplicatePermissionExists_ShouldThrowValidationException()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";
        var buIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", cancellationToken);

        Task<SecurityIdentity> Assign() => this.AuthManager.For(principalName)
            .AddRoleAsync(new ExampleTestPermission(ExampleRoles.BuManager) { BusinessUnit = buIdentity }, cancellationToken);

        await Assign();

        // Act
        var action = Assign;

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should().Contain($"Principal \"{principalName}\" has duplicate permissions");
    }


    [Fact]
    public async Task AddRoleAsync_WhenPermissionPeriodsDoNotIntersect_ShouldNotThrow()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";
        var buIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", cancellationToken);

        Task<SecurityIdentity> Assign(PermissionPeriod period) => this.AuthManager.For(principalName)
            .AddRoleAsync(new ExampleTestPermission(ExampleRoles.BuManager) { BusinessUnit = buIdentity, Period = period }, cancellationToken);

        await Assign(new PermissionPeriod(new DateTime(2000, 1, 1), new DateTime(2009, 1, 1)));

        // Act
        var action = () => Assign(new PermissionPeriod(new DateTime(2010, 1, 1), new DateTime(2019, 1, 1)));

        // Assert
        var error = await action.Should().NotThrowAsync();
    }
}