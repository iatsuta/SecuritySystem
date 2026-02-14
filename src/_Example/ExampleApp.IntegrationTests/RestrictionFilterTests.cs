using ExampleApp.Application;
using ExampleApp.Domain;
using SecuritySystem;
using SecuritySystem.Testing;
using SecuritySystem.Validation;

namespace ExampleApp.IntegrationTests;

public class RestrictionFilterTests : TestBase
{
    private readonly SecurityRole securityRole = ExampleRoles.DefaultRole;

    private readonly SecurityRule restrictionRule = ExampleRoles.DefaultRole.ToSecurityRule(
        customRestriction: SecurityPathRestriction.Default.Add<BusinessUnit>(filter: bu => bu.AllowedForFilterRole));

    private readonly string testLogin = "RestrictionFilterTests";

    private TypedSecurityIdentity<Guid> defaultBu = null!;

    private TypedSecurityIdentity<Guid> buWithAllowedFilter = null!;


    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        this.defaultBu = await this.AuthManager.SaveSecurityContextAsync<BusinessUnit, Guid>(
            () => new BusinessUnit { Name = nameof(this.defaultBu) }, this.CancellationToken);

        this.buWithAllowedFilter = await this.AuthManager.SaveSecurityContextAsync<BusinessUnit, Guid>(
            () => new BusinessUnit { Name = nameof(this.buWithAllowedFilter), AllowedForFilterRole = true }, this.CancellationToken);
    }

    [Fact]
    public async Task CreatePermissionWithRestrictionFilter_ApplyInvalidBusinessUnit_ExceptionRaised()
    {
        // Arrange

        // Act
        var action = () => this.AuthManager.For(this.testLogin).SetRoleAsync(
            new TestPermission(ExampleRoles.WithRestrictionFilterRole)
            {
                BusinessUnit = this.defaultBu
            }, this.CancellationToken);

        // Assert
        var error = await action.Should().ThrowAsync<SecuritySystemValidationException>();

        error.And.Message.Should().Contain($"SecurityContext: '{this.defaultBu.Id}' denied by filter");
    }

    [Fact]
    public async Task CreatePermissionWithRestrictionFilter_ApplyCorrectBusinessUnit_ExceptionNotRaised()
    {
        // Arrange

        // Act
        var action = () => this.AuthManager.For(this.testLogin).SetRoleAsync(
                         new TestPermission(ExampleRoles.WithRestrictionFilterRole)
                         {
                             BusinessUnit = this.buWithAllowedFilter
                         }, this.CancellationToken);

        // Assert
        await action.Should().NotThrowAsync();
    }


    [Fact]
    public async Task CreateCustomRestrictionRule_ApplyGrandPermission_OnlyCorrectBuFounded()
    {
        // Arrange
        await this.AuthManager.For(this.testLogin).SetRoleAsync(this.securityRole, this.CancellationToken);
        this.AuthManager.For(this.testLogin).LoginAs();

        // Act
        var allowedBuList = await this.AuthManager.GetIdentityListAsync<BusinessUnit, Guid>(this.restrictionRule, this.CancellationToken);

        // Assert
        allowedBuList.Should().BeEquivalentTo([this.buWithAllowedFilter]);
    }

    [Fact]
    public async Task CreateCustomRestrictionRule_ApplySingleCorrectBU_OnlyCorrectBuFounded()
    {
        // Arrange
        await this.AuthManager.For(this.testLogin).SetRoleAsync(new TestPermission(this.securityRole) { BusinessUnits = [this.defaultBu, this.buWithAllowedFilter] }, this.CancellationToken);
        this.AuthManager.For(this.testLogin).LoginAs();

        // Act
        var allowedBuList = await this.AuthManager.GetIdentityListAsync<BusinessUnit, Guid>(this.restrictionRule, this.CancellationToken);

        // Assert
        allowedBuList.Should().BeEquivalentTo([this.buWithAllowedFilter]);
    }

    //[Fact]
    //public async Task CreateCustomRestrictionRule_SearchAccessorsForGrandPermission_EmployeeFounded()
    //{
    //    // Arrange
    //    await this.AuthManager.For(this.testLogin).SetRoleAsync(this.securityRole, this.CancellationToken);

    //    // Act
    //    await using var scope = this.RootServiceProvider.CreateAsyncScope();

    //    var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();
    //    var domainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<BusinessUnit>>();
    //    var securityAccessorResolver = scope.ServiceProvider.GetRequiredService<ISecurityAccessorResolver>();

    //    var bu = await queryableSource.GetQueryable<BusinessUnit>().Where(bu => bu.Id == this.buWithAllowedFilter.Id).GenericSingleAsync(this.CancellationToken);

    //    var accessorData = domainSecurityService.GetSecurityProvider(this.restrictionRule).GetAccessorData(bu);

    //    var accessors = securityAccessorResolver.Resolve(accessorData).ToList();

    //    // Assert
    //    accessors.Should().Contain(this.testLogin);
    //}

    //[Fact]
    //public async Task CreateCustomRestrictionRule_SearchAccessorsForCorrectBU_EmployeeFounded()
    //{
    //    // Arrange
    //    await this.AuthManager.For(this.testLogin)
    //        .SetRoleAsync(new TestPermissionBuilder(this.securityRole) { BusinessUnits = [this.defaultBu, this.buWithAllowedFilter] },
    //            this.CancellationToken);

    //    // Act
    //    await using var scope = this.RootServiceProvider.CreateAsyncScope();

    //    var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();
    //    var domainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<BusinessUnit>>();
    //    var securityAccessorResolver = scope.ServiceProvider.GetRequiredService<ISecurityAccessorResolver>();

    //    var bu = await queryableSource.GetQueryable<BusinessUnit>().Where(bu => bu.Id == this.buWithAllowedFilter.Id).GenericSingleAsync(this.CancellationToken);

    //    var accessorData = domainSecurityService.GetSecurityProvider(this.restrictionRule).GetAccessorData(bu);

    //    var accessors = securityAccessorResolver.Resolve(accessorData).ToList();

    //    // Assert
    //    accessors.Should().Contain(this.testLogin);
    //}

    //[Fact]
    //public async Task CreateCustomRestrictionRule_SearchAccessorsForIncorrectBU_EmployeeNotFounded()
    //{
    //    // Arrange
    //    await this.AuthManager.For(this.testLogin)
    //        .SetRoleAsync(new TestPermissionBuilder(this.securityRole) { BusinessUnits = [this.defaultBu, this.buWithAllowedFilter] },
    //            this.CancellationToken);

    //    // Act
    //    await using var scope = this.RootServiceProvider.CreateAsyncScope();

    //    var queryableSource = scope.ServiceProvider.GetRequiredService<IQueryableSource>();
    //    var domainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<BusinessUnit>>();
    //    var securityAccessorResolver = scope.ServiceProvider.GetRequiredService<ISecurityAccessorResolver>();

    //    var bu = await queryableSource.GetQueryable<BusinessUnit>().Where(bu => bu.Id == this.defaultBu.Id).GenericSingleAsync(this.CancellationToken);

    //    var accessorData = domainSecurityService.GetSecurityProvider(this.restrictionRule).GetAccessorData(bu);

    //    var accessors = securityAccessorResolver.Resolve(accessorData).ToList();

    //    // Assert
    //    accessors.Should().NotContainInConsecutiveOrder(this.testLogin);
    //}
}
