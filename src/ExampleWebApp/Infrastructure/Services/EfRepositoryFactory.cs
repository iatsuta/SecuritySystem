using ExampleWebApp.Application;

using SecuritySystem;
using SecuritySystem.DomainServices;

namespace ExampleWebApp.Infrastructure.Services;

public class EfRepositoryFactory<TDomainObject>(IServiceProvider serviceProvider, IDomainSecurityService<TDomainObject> domainSecurityService)
    : IRepositoryFactory<TDomainObject> where TDomainObject : class
{
    public IRepository<TDomainObject> Create(SecurityRule securityRule)
    {
        return ActivatorUtilities.CreateInstance<EfRepository<TDomainObject>>(serviceProvider, domainSecurityService.GetSecurityProvider(securityRule));
    }
}