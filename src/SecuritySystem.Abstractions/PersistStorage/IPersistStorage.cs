namespace SecuritySystem.PersistStorage;

public interface IStorageWriter
{
    Task SaveAsync<TDomainObject>(TDomainObject data, CancellationToken cancellationToken)
        where TDomainObject : class;
}
