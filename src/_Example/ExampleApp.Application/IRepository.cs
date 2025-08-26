namespace ExampleWebApp.Application;

public interface IRepository<TDomainObject>
{
    Task UpdateAsync(TDomainObject domainObject, CancellationToken cancellationToken = default);

    Task InsertAsync(TDomainObject domainObject, CancellationToken cancellationToken = default);

    Task RemoveAsync(TDomainObject domainObject, CancellationToken cancellationToken = default);

    IQueryable<TDomainObject> GetQueryable();
}