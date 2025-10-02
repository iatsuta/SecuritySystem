namespace SecuritySystem.Services;

public interface IDenormalizedAncestorsService<in TDomainObject>
{
    Task SyncUpAsync(TDomainObject domainObject, CancellationToken cancellationToken);

    Task SyncAllAsync(CancellationToken cancellationToken);

    Task SyncAsync(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken);
}