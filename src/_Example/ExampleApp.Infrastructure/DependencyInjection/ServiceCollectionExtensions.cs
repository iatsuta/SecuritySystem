using ExampleApp.Application;
using ExampleApp.Domain;
using ExampleApp.Domain.Auth;
using ExampleApp.Infrastructure.Services;

using GenericQueryable.EntityFramework;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;
using SecuritySystem.DependencyInjection;
using SecuritySystem.VirtualPermission;
using System.Threading;

namespace ExampleApp.Infrastructure.DependencyInjection;

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
                    .AddSecurityContext<Location>(new Guid("{9756440C-6643-4AAD-AB57-A901F3917BA4}"), scb => scb.SetDisplayFunc(loc => loc.Name).SetIdentityPath(loc => loc.MyId))

                    .AddDomainSecurityServices(rb =>
                        rb.Add<TestObject>(b => b.SetView(ExampleRoles.TestManager)
                            .SetPath(SecurityPath<TestObject>.Create(testObj => testObj.BusinessUnit).And(testObj => testObj.Location))))

                    .AddSecurityRole(ExampleRoles.TestManager, new SecurityRoleInfo(new Guid("{72D24BB5-F661-446A-A458-53D301805971}")))
                    .AddSecurityRole(SecurityRole.Administrator, new SecurityRoleInfo(new Guid("{2573CFDC-91CD-4729-AE97-82AB2F235E23}")))

                    .AddVirtualPermission<Employee, TestManager>(
                        ExampleRoles.TestManager, domainObject => domainObject.Employee, employee => employee.Login,
                        bi => bi
                            .AddRestriction(domainObject => domainObject.BusinessUnit)
                            .AddRestriction(domainObject => domainObject.Location))

                    .AddVirtualPermission<Employee, Administrator>(
                        SecurityRole.Administrator, domainObject => domainObject.Employee, employee => employee.Login)

                    
            );
    }
}