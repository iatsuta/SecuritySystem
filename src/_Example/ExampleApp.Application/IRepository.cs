namespace ExampleApp.Application;

public interface IRepository<TDomainObject>
{
    Task SaveAsync(TDomainObject domainObject, CancellationToken cancellationToken = default);

    Task RemoveAsync(TDomainObject domainObject, CancellationToken cancellationToken = default);

    IQueryable<TDomainObject> GetQueryable();
}