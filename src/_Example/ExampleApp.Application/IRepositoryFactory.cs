using SecuritySystem;

namespace ExampleWebApp.Application;

public interface IRepositoryFactory<TDomainObject>
{
    IRepository<TDomainObject> Create() => this.Create(SecurityRule.Disabled);

    IRepository<TDomainObject> Create(SecurityRule securityRule);
}