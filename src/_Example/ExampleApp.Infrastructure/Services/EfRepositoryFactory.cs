using CommonFramework;

using ExampleApp.Application;

using SecuritySystem;
using SecuritySystem.DomainServices;

namespace ExampleApp.Infrastructure.Services;

public class EfRepositoryFactory<TDomainObject>(
    IServiceProxyFactory serviceProxyFactory,
    IDomainSecurityService<TDomainObject> domainSecurityService)
    : IRepositoryFactory<TDomainObject>
    where TDomainObject : class
{
    public IRepository<TDomainObject> Create(SecurityRule securityRule) =>
        serviceProxyFactory.Create<
            IRepository<TDomainObject>, EfRepository<TDomainObject>>(
            domainSecurityService.GetSecurityProvider(securityRule));
}