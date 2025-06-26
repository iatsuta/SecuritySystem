using ExampleWebApp.Application;
using ExampleWebApp.Domain;
using ExampleWebApp.Domain.Auth;
using ExampleWebApp.Infrastructure.Services;

using GenericQueryable.EntityFramework;

using Microsoft.EntityFrameworkCore;

using SecuritySystem;
using SecuritySystem.DependencyInjection;
using SecuritySystem.VirtualPermission;

namespace ExampleWebApp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddHttpContextAccessor()
            .AddDbContext<TestDbContext>(optionsBuilder => optionsBuilder
                .UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                .UseLazyLoadingProxies()
                .UseGenericQueryable())
            .AddSecuritySystem()
            .AddRepository();
    }

    private static IServiceCollection AddRepository(this IServiceCollection services)
    {
        return services
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddScoped(typeof(IRepositoryFactory<>), typeof(EfRepositoryFactory<>));
    }

    private static IServiceCollection AddSecuritySystem(this IServiceCollection services)
    {
        return services
            .AddSecuritySystem(sss =>
                sss
                    .SetQueryableSource<EfQueryableSource>()
                    .SetRawUserAuthenticationService<RawUserAuthenticationService>()
                    .SetStorageWriter<EfStorageWriter>()

                    .SetUserSource<Employee>(employee => employee.Id, employee => employee.Login, _ => true, employee => employee.RunAs)

                    .AddSecurityContext<BusinessUnit>(new Guid("{E4AE968E-7B6B-4236-B381-9886C8E0FA34}"), scb => scb.SetDisplayFunc(bu => bu.Name))

                    .AddDomainSecurityServices(rb =>
                        rb.Add<TestObject>(b => b.SetView(ExampleRoles.TestManager).SetPath(SecurityPath<TestObject>.Create(testObj => testObj.BusinessUnit))))

                    .AddSecurityRole(ExampleRoles.TestManager, new SecurityRoleInfo(new Guid("{72D24BB5-F661-446A-A458-53D301805971}")))
                    .AddSecurityRole(SecurityRole.Administrator, new SecurityRoleInfo(new Guid("{2573CFDC-91CD-4729-AE97-82AB2F235E23}")))

                    .AddVirtualPermission<Employee, TestManager>(
                        ExampleRoles.TestManager, domainObject => domainObject.Employee, employee => employee.Login,
                        bi => bi.AddRestriction(domainObject => domainObject.BusinessUnit))

                    .AddVirtualPermission<Employee, Administrator>(
                        SecurityRole.Administrator, domainObject => domainObject.Employee, employee => employee.Login)
            );
    }
}