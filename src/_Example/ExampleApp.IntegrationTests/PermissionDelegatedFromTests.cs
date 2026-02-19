using ExampleApp.Application;
using ExampleApp.Domain;

using SecuritySystem;
using SecuritySystem.Testing;
using SecuritySystem.Validation;

namespace ExampleApp.IntegrationTests;

public class PermissionDelegationFromTests : TestBase
{
    [Fact]
    public async Task SetRoleAsync_ShouldPreserveDelegatedFromIdentity()
    {
        // Arrange
        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole) { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()) };

        await this.AuthManager.For("DelegatedFromPrincipal").SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        var subPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), DelegatedFrom = delegatedFromPermission.Identity };

        // Act
        var principalIdentity = await this.AuthManager.For("TargetPrincipal").SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var managedPrincipal = await this.AuthManager.For(principalIdentity).GetPrincipalAsync(this.CancellationToken);

        var managedPermission = managedPrincipal.Permissions.Should().ContainSingle().Subject;

        managedPermission.Identity.Should().Be(subPermission.Identity);
        managedPermission.DelegatedFrom.Should().Be(subPermission.DelegatedFrom);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldThrow_WhenDelegatingToOriginalPrincipal()
    {
        // Arrange
        var delegatedFromPermission = new TestPermission(SecurityRole.Administrator)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()) };

        var principalIdentity = await this.AuthManager.For("DelegatedFromPrincipal").SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        var subPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), DelegatedFrom = delegatedFromPermission.Identity };

        // Act
        var action = () => this.AuthManager.For(principalIdentity).AddRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should().Be("Invalid delegation target: the permission cannot be delegated to its original principal");
    }

    [Fact]
    public async Task SetRoleAsync_ShouldPreserveDelegatedFrom_WhenAssigningToChildBusinessUnit()
    {
        // Arrange
        var rootBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", this.CancellationToken);
        var childBuIdentity =
            await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>($"Test{nameof(BusinessUnit)}1", this.CancellationToken);

        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = rootBuIdentity };

        await this.AuthManager.For("DelegatedFromPrincipal").SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        var subPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = childBuIdentity, DelegatedFrom = delegatedFromPermission.Identity };

        // Act
        var principalIdentity = await this.AuthManager.For("TargetPrincipal").SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var managedPrincipal = await this.AuthManager.For(principalIdentity).GetPrincipalAsync(this.CancellationToken);

        var managedPermission = managedPrincipal.Permissions.Should().ContainSingle().Subject;

        managedPermission.Identity.Should().Be(subPermission.Identity);
        managedPermission.DelegatedFrom.Should().Be(subPermission.DelegatedFrom);
        managedPermission.Restrictions.Should().BeEquivalentTo(subPermission.Restrictions);
    }


    [Fact]
    public async Task Test4()
    {
        // Arrange
        var rootBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", this.CancellationToken);

        var delegatedFromPermission = new TestPermission(ExampleRoles.BuManager)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = rootBuIdentity };

        await this.AuthManager.For("DelegatedFromPrincipal").SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        var subPermission = new TestPermission(ExampleRoles.BuManager)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), DelegatedFrom = delegatedFromPermission.Identity };

        // Act
        var action = () => this.AuthManager.For("TargetPrincipal").SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should()
            .Be(
                $"Invalid security context delegation: the source principal \"{0}\" does not have access to the following security contexts required for delegation to \"{1}\": {2}");
    }
}