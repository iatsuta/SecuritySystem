using ExampleApp.Application;
using ExampleApp.Domain;

using SecuritySystem;
using SecuritySystem.GeneralPermission;
using SecuritySystem.Testing;
using SecuritySystem.Validation;

namespace ExampleApp.IntegrationTests;

public class PermissionDelegationFromTests : TestBase
{
    [Fact]
    public async Task SetRoleAsync_ShouldPreserveDelegatedFromIdentity()
    {
        // Arrange
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";
        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole) { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()) };

        var subPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), DelegatedFrom = delegatedFromPermission.Identity };


        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var principalIdentity = await this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

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

        var subPermission = new TestPermission(ExampleRoles.DefaultRole) { DelegatedFrom = delegatedFromPermission.Identity };


        var principalIdentity = await this.AuthManager.For("DelegatedFromPrincipal").SetRoleAsync(delegatedFromPermission, this.CancellationToken);

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
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";
        var sourceBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", this.CancellationToken);
        var targetBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>($"Test{nameof(BusinessUnit)}1", this.CancellationToken);

        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = sourceBuIdentity };

        var subPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = targetBuIdentity, DelegatedFrom = delegatedFromPermission.Identity };


        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var principalIdentity = await this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var managedPrincipal = await this.AuthManager.For(principalIdentity).GetPrincipalAsync(this.CancellationToken);

        var managedPermission = managedPrincipal.Permissions.Should().ContainSingle().Subject;

        managedPermission.Identity.Should().Be(subPermission.Identity);
        managedPermission.DelegatedFrom.Should().Be(subPermission.DelegatedFrom);
        managedPermission.Restrictions.Should().BeEquivalentTo(subPermission.Restrictions);
    }

    [Fact]
    public async Task SetRoleAsync_ShouldThrow_WhenDelegationExceedsSourceBusinessUnit()
    {
        // Arrange
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";
        var sourceBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", this.CancellationToken);
        var invalidObjects = $"{nameof(BusinessUnit)}: Unrestricted";

        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = sourceBuIdentity };

        var subPermission = new TestPermission(ExampleRoles.DefaultRole) { DelegatedFrom = delegatedFromPermission.Identity };


        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var action = () => this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should()
            .Be(
                $"Invalid security context delegation: the security contexts of \"{targetPrincipalName}\" exceed those granted by \"{sourcePrincipalName}\": {invalidObjects}");
    }

    [Fact]
    public async Task SetRoleAsync_WhenTargetBusinessUnitExceedsSource_ShouldFail()
    {
        // Arrange
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";
        var sourceBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>($"Test{nameof(BusinessUnit)}1", this.CancellationToken);
        var targetBuIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>($"Test{nameof(BusinessUnit)}2", this.CancellationToken);
        var invalidObjects = $"{nameof(BusinessUnit)}: {targetBuIdentity.Id}";

        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), BusinessUnit = sourceBuIdentity };

        var subPermission = new TestPermission(ExampleRoles.DefaultRole) { BusinessUnit = targetBuIdentity, DelegatedFrom = delegatedFromPermission.Identity };


        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var action = () => this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should()
            .Be(
                $"Invalid security context delegation: the security contexts of \"{targetPrincipalName}\" exceed those granted by \"{sourcePrincipalName}\": {invalidObjects}");
    }

    [Fact]
    public async Task SetRoleAsync_ShouldThrow_WhenDelegatedRoleIsNotSubsetOfSource()
    {
        // Arrange
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";

        var sourceRole = ExampleRoles.DefaultRole;
        var targetRole = SecurityRole.Administrator;

        var delegatedFromPermission = new TestPermission(sourceRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()) };

        var subPermission = new TestPermission(targetRole) { DelegatedFrom = delegatedFromPermission.Identity };

        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var action = () => this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should()
            .Be(
                $"Invalid delegated permission role: the selected role \"{targetRole}\" is not a subset of \"{sourceRole}\"");
    }

    [Fact]
    public async Task SetRoleAsync_ShouldThrow_WhenDelegatedPeriodIsNotSubsetOfSource()
    {
        // Arrange
        var sourcePrincipalName = "DelegatedFromPrincipal";
        var targetPrincipalName = "TargetPrincipal";

        var today = DateTime.Today;

        var sourcePeriod = new PermissionPeriod(today, today);
        var targetPeriod = PermissionPeriod.Eternity with { StartDate = DateTime.MinValue };

        var expectedErrorMessage = $"Invalid delegated permission period: the selected period \"{targetPeriod}\" is not a subset of \"{sourcePeriod}\"";

        var delegatedFromPermission = new TestPermission(ExampleRoles.DefaultRole)
            { Identity = TypedSecurityIdentity.Create(Guid.NewGuid()), Period = sourcePeriod };

        var subPermission = new TestPermission(ExampleRoles.DefaultRole) { Period = targetPeriod, DelegatedFrom = delegatedFromPermission.Identity };

        await this.AuthManager.For(sourcePrincipalName).SetRoleAsync(delegatedFromPermission, this.CancellationToken);

        // Act
        var action = () => this.AuthManager.For(targetPrincipalName).SetRoleAsync(subPermission, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should().Be(expectedErrorMessage);
    }
}