namespace SecuritySystem.Services;

public interface IGenericRepository
{
    Task SaveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class;

    Task RemoveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class;
}
