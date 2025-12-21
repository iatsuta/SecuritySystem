using CommonFramework.GenericRepository;
using ExampleApp.Application;
using ExampleApp.Domain;
using GenericQueryable;
using Microsoft.Extensions.DependencyInjection;
using SecuritySystem;
using SecuritySystem.AvailableSecurity;
using SecuritySystem.DomainServices;
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
        authenticationService.CurrentUserName = principalName;

        var availableSecurityRoleSource = scope.ServiceProvider.GetRequiredService<IAvailableSecurityRoleSource>();

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

    [Fact]
    public async Task AssignGeneralPermission_WithRootBu_AllTestObjectsResolved()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var principalName = "TestPrincipal";

        var buIdentity = await this.AuthManager.GetSecurityContextIdentityAsync<BusinessUnit, Guid>("TestRootBu", cancellationToken);

        var testRole = ExampleRoles.BuManager;

        var testPermission = new ExampleTestPermission(testRole) { BusinessUnit = buIdentity };

        await this.AuthManager.For(principalName).SetRoleAsync(testPermission, cancellationToken);

        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var testObjectDomainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<TestObject>>();
        var securityProvider = testObjectDomainSecurityService.GetSecurityProvider(testRole);

        var authenticationService = scope.ServiceProvider.GetRequiredService<TestRawUserAuthenticationService>();
        authenticationService.CurrentUserName = principalName;

        var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();

        var testObjectRepositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFactory<TestObject>>();
        var testObjectRepository = testObjectRepositoryFactory.Create(testRole);

        var expectedResult = await queryableSource.GetQueryable<TestObject>().GenericToListAsync(cancellationToken);

        // Act
        var result = await testObjectRepository.GetQueryable().GenericToListAsync(cancellationToken);

        // Assert
        result.OrderBy(v => v.Id).Should().BeEquivalentTo(expectedResult.OrderBy(v => v.Id));

        foreach (var testObject in result)
        {
            securityProvider.HasAccess(testObject).Should().Be(true);
        }
    }
}