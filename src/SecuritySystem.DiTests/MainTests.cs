using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using SecuritySystem.DiTests.DomainObjects;
using SecuritySystem.DiTests.Rules;
using SecuritySystem.DiTests.Services;
using SecuritySystem.DomainServices;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Providers;
using SecuritySystem.Services;

namespace SecuritySystem.DiTests;

public class MainTests : TestBase
{
    private readonly BusinessUnit bu1;

    private readonly BusinessUnit bu2;

    private readonly BusinessUnit bu3;

    private readonly Employee employee1;

    private readonly Employee employee2;

    private readonly Employee employee3;

    private readonly Employee employee4;

    public MainTests()
    {
        this.bu1 = new BusinessUnit() { Id = Guid.NewGuid() };
        this.bu2 = new BusinessUnit() { Id = Guid.NewGuid(), Parent = this.bu1 };
        this.bu3 = new BusinessUnit() { Id = Guid.NewGuid() };

        this.employee1 = new Employee() { Id = Guid.NewGuid(), BusinessUnit = this.bu1 };
        this.employee2 = new Employee() { Id = Guid.NewGuid(), BusinessUnit = this.bu2 };
        this.employee3 = new Employee() { Id = Guid.NewGuid(), BusinessUnit = this.bu3 };
        this.employee4 = new Employee() { Id = Guid.NewGuid() };
    }

    protected override IServiceCollection CreateServices(IServiceCollection serviceCollection)
    {
        return base.CreateServices(serviceCollection)
            .AddScoped<BusinessUnitAncestorLinkSourceExecuteCounter>()
            .Replace(ServiceDescriptor.Scoped<IQueryableSource>(sp => new TestQueryableSource { BaseQueryableSource = this.BuildQueryableSource(sp) }));
    }

    protected override IEnumerable<TestPermission> GetPermissions()
    {
        yield return new TestPermission(
            ExampleSecurityRole.TestRole,
            new Dictionary<Type, IReadOnlyList<Guid>> { { typeof(BusinessUnit), [this.bu1.Id] } });
    }

    [Fact]
    public async Task TestEmployeesSecurity_EmployeeHasAccessCorrect()
    {
        // Arrange
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var employeeDomainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<Employee>>();
        var counterService = scope.ServiceProvider.GetRequiredService<BusinessUnitAncestorLinkSourceExecuteCounter>();
        var securityProvider = employeeDomainSecurityService.GetSecurityProvider(SecurityRule.View);

        // Act
        var result1 = securityProvider.HasAccess(this.employee1);
        var result2 = securityProvider.HasAccess(this.employee2);
        var result3 = securityProvider.HasAccess(this.employee3);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
        result3.Should().BeFalse();

        counterService.Count.Should().Be(1);
    }

    [Fact]
    public async Task CheckEmployeeWithoutSecurity_ExceptionRaised()
    {
        // Arrange
        await using var scope = this.RootServiceProvider.CreateAsyncScope();

        var employeeDomainSecurityService = scope.ServiceProvider.GetRequiredService<IDomainSecurityService<Employee>>();
        var accessDeniedExceptionService = scope.ServiceProvider.GetRequiredService<IAccessDeniedExceptionService>();

        var securityProvider = employeeDomainSecurityService.GetSecurityProvider(SecurityRule.View);

        // Act
        var checkAccessAction = () => securityProvider.CheckAccess(this.employee3, accessDeniedExceptionService);

        // Assert
        checkAccessAction.Should().Throw<AccessDeniedException>();
    }

    protected IQueryableSource BuildQueryableSource(IServiceProvider serviceProvider)
    {
        var queryableSource = Substitute.For<IQueryableSource>();

        queryableSource.GetQueryable<BusinessUnitAncestorLink>()
                       .Returns(this.GetBusinessUnitAncestorLinkSource(serviceProvider).AsQueryable());

        queryableSource.GetQueryable<Employee>().Returns(new[] { this.employee1, this.employee2, this.employee3, this.employee4 }.AsQueryable());

        return queryableSource;
    }

    private IEnumerable<BusinessUnitAncestorLink> GetBusinessUnitAncestorLinkSource(IServiceProvider serviceProvider)
    {
        var counter = serviceProvider.GetRequiredService<BusinessUnitAncestorLinkSourceExecuteCounter>();
        counter.Count++;

        yield return new BusinessUnitAncestorLink { Ancestor = this.bu1, Child = this.bu1 };
        yield return new BusinessUnitAncestorLink { Ancestor = this.bu2, Child = this.bu2 };
        yield return new BusinessUnitAncestorLink { Ancestor = this.bu3, Child = this.bu3 };

        yield return new BusinessUnitAncestorLink { Ancestor = this.bu1, Child = this.bu2 };
    }

    private class BusinessUnitAncestorLinkSourceExecuteCounter
    {
        public int Count { get; set; }
    }
}
