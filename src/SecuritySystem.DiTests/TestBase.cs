using CommonFramework;
using CommonFramework.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DependencyInjection;
using SecuritySystem.DiTests.DomainObjects;
using SecuritySystem.DiTests.Rules;
using SecuritySystem.DiTests.Services;
using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.DiTests;

public abstract class TestBase
{
    private readonly Lazy<IServiceProvider> lazyRootServiceProvider;

    protected TestBase()
    {
        this.lazyRootServiceProvider = LazyHelper.Create(() => this.BuildRootServiceProvider(new ServiceCollection()));
    }

    protected IServiceProvider RootServiceProvider => this.lazyRootServiceProvider.Value;

    protected virtual IEnumerable<TestPermission> GetPermissions() => [];

    protected virtual IServiceProvider BuildRootServiceProvider(IServiceCollection serviceCollection)
    {
        return serviceCollection

               //.RegisterHierarchicalObjectExpander()
               .AddScoped(this.BuildQueryableSource)

               .AddSecuritySystem(
                   settings =>

                       settings
                           .AddPermissionSystem<ExamplePermissionSystemFactory>()

                           .AddDomainSecurityServices(
                               rb =>
                                   rb.Add<Employee>(
                                       b => b.SetView(ExampleSecurityOperation.EmployeeView)
                                             .SetEdit(ExampleSecurityOperation.EmployeeEdit)
                                             .SetPath(SecurityPath<Employee>.Create(v => v.BusinessUnit))))

                           .AddSecurityContext<BusinessUnit>(Guid
                                   .NewGuid(),
                               scb => scb.SetHierarchicalInfo(
                                    bu => bu.Parent,
                                    new AncestorLinkInfo<BusinessUnit, BusinessUnitAncestorLink>(bu => bu.Ancestor, bu => bu.Child),
                                    new AncestorLinkInfo<BusinessUnit, BusinessUnitToAncestorChildView>(bu => bu.Source, bu => bu.ChildOrAncestor)))

                           .AddSecurityContext<Location>(Guid.NewGuid())

                           .AddSecurityRole(
                               ExampleSecurityRole.TestRole,
                               new SecurityRoleInfo(Guid.NewGuid())
                               {
                                   Children = [ExampleSecurityRole.TestRole2],
                                   Operations = [ExampleSecurityOperation.EmployeeView, ExampleSecurityOperation.EmployeeEdit]
                               })

                           .AddSecurityRole(
                               ExampleSecurityRole.TestRole2,
                               new SecurityRoleInfo(Guid.NewGuid()) { Children = [ExampleSecurityRole.TestRole3] })

                           .AddSecurityRole(
                               ExampleSecurityRole.TestRole3,
                               new SecurityRoleInfo(Guid.NewGuid()))

                           .AddSecurityRole(
                               ExampleSecurityRole.TestRole4,
                               new SecurityRoleInfo(Guid.NewGuid()) { Operations = [ExampleSecurityOperation.BusinessUnitView] })

                           .AddSecurityRole(
                               ExampleSecurityRole.TestKeyedRole,
                               new SecurityRoleInfo(Guid.NewGuid()) { Restriction = SecurityPathRestriction.Create<Location>().Add<BusinessUnit>(key: "testKey") })

                           .AddSecurityRole(SecurityRole.Administrator, new SecurityRoleInfo(Guid.NewGuid()))

                           .AddSecurityOperation(
                               ExampleSecurityOperation.BusinessUnitView,
                               new SecurityOperationInfo { CustomExpandType = HierarchicalExpandType.None })

                   )

               .AddRelativeDomainPath((Employee employee) => employee)
               .AddSingleton(typeof(TestCheckboxConditionFactory<>))
               .AddScoped<IRawUserAuthenticationService, FakeRawUserAuthenticationService>()

               .AddSingleton(_ => new TestPermissionData(this.GetPermissions().ToList()))

               .ValidateDuplicateDeclaration()
               .BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }

    protected virtual IQueryableSource BuildQueryableSource(IServiceProvider serviceProvider)
    {
        return Substitute.For<IQueryableSource>();
    }
}
