using ExampleApp.Application;

using SecuritySystem.Testing;

namespace ExampleApp.IntegrationTests;

public class PermissionExtendedDataTests : TestBase
{
    [Fact]
    public async Task SetRoleAsync_WithExtendedValue_ShouldPersistExtendedData()
    {
        // Arrange
        var principalName = "TestPrincipal";

        var extendedValue = "abc";

        var testPermission = new TestPermission(ExampleRoles.DefaultRole) { ExtendedValue = extendedValue }.ToManagedPermissionData();

        // Act
        var principalIdentity = await this.AuthManager.For(principalName).SetRoleAsync(testPermission, this.CancellationToken);

        // Assert
        var managedPrincipal = await this.AuthManager.For(principalIdentity).GetPrincipalAsync(this.CancellationToken);

        var managedPermission = managedPrincipal.Permissions.Should().ContainSingle().Subject;

        managedPermission.ExtendedData.GetValueOrDefault(TestPermissionExtensions.ExtendedKey)
            .Should().Be(extendedValue);
    }
}