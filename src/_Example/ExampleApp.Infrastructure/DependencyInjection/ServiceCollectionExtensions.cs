using ExampleApp.Application;
using ExampleApp.Domain;
using ExampleApp.Domain.Auth.Virtual;
using ExampleApp.Infrastructure.Services;
using AuthGeneral = ExampleApp.Domain.Auth.General;

using GenericQueryable.EntityFramework;

using HierarchicalExpand;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem;
using SecuritySystem.DependencyInjection;

using SecuritySystem.GeneralPermission.DependencyInjection;
using SecuritySystem.UserSource;
using SecuritySystem.VirtualPermission.DependencyInjection;

namespace ExampleApp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            return services
                .AddLogging()
                .AddHttpContextAccessor()
                .AddDbContext<TestDbContext>(optionsBuilder => optionsBuilder
                    .UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                    .UseLazyLoadingProxies()
                    .UseGenericQueryable())
                .AddSecuritySystem()
                .AddRepository();
        }

        private IServiceCollection AddRepository()
        {
            return services
                .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
                .AddScoped(typeof(IRepositoryFactory<>), typeof(EfRepositoryFactory<>));
        }

        private IServiceCollection AddSecuritySystem()
        {
            return services
                .AddSecuritySystem(sss =>
                    sss
                        .SetQueryableSource<EfQueryableSource>()
                        .SetGenericRepository<EfGenericRepository>()
                        .SetRawUserAuthenticationService<RawUserAuthenticationService>()

                        .AddUserSource<Employee>(usb => usb.SetRunAs(employee => employee.RunAs))
                        .AddUserSource<AuthGeneral.Principal>(usb => usb.SetMissedService<CreateVirtualMissedUserService<AuthGeneral.Principal>>())

                        .AddSecurityContext<BusinessUnit>(
                            new Guid("{E4AE968E-7B6B-4236-B381-9886C8E0FA34}"),
                            scb => scb
                                .SetHierarchicalInfo(
                                    v => v.Parent,
                                    new AncestorLinkInfo<BusinessUnit, BusinessUnitDirectAncestorLink>(link => link.Ancestor, link => link.Child),
                                    new AncestorLinkInfo<BusinessUnit, BusinessUnitUndirectAncestorLink>(view => view.Source, view => view.Target)))
                        .AddSecurityContext<Location>(
                            new Guid("{9756440C-6643-4AAD-AB57-A901F3917BA4}"),
                            scb => scb
                                .SetIdentityPath(loc => loc.MyId))

                        .AddDomainSecurityServices(rb =>
                            rb.Add<TestObject>(b => b
                                    .SetView(ExampleRoles.TestManager)
                                    .SetPath(SecurityPath<TestObject>.Create(testObj => testObj.BusinessUnit).And(testObj => testObj.Location)))
                                .Add<Employee>(b => b
                                    .SetView(DomainSecurityRule.CurrentUser))
                                .Add<BusinessUnit>(b => b
                                    .SetView(ExampleRoles.TestManager.ToSecurityRule(HierarchicalExpandType.All))
                                    .SetPath(SecurityPath<BusinessUnit>.Create(v => v))))

                        .AddSecurityRole(ExampleRoles.TestManager, new SecurityRoleInfo(new Guid("{72D24BB5-F661-446A-A458-53D301805971}")))
                        .AddSecurityRole(SecurityRole.Administrator, new SecurityRoleInfo(new Guid("{2573CFDC-91CD-4729-AE97-82AB2F235E23}")))

                        .AddVirtualPermission<Employee, TestManager>(
                            ExampleRoles.TestManager, domainObject => domainObject.Employee,
                            bi => bi
                                .AddRestriction(domainObject => domainObject.BusinessUnit)
                                .AddRestriction(domainObject => domainObject.Location))

                        .AddVirtualPermission<Employee, Administrator>(
                            SecurityRole.Administrator, domainObject => domainObject.Employee)

                        .AddGeneralPermission(
                            p => p.Principal,
                            p => p.SecurityRole,
                            (AuthGeneral.PermissionRestriction pr) => pr.Permission,
                            pr => pr.SecurityContextType,
                            pr => pr.SecurityContextId,
                            b => b.SetSecurityRoleDescription(sr => sr.Description)));
        }
    }
}