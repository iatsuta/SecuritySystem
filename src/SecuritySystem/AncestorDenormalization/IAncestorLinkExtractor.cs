namespace SecuritySystem.AncestorDenormalization;

public interface IAncestorLinkExtractor<TDomainObject, TDomainObjectAncestorLink>
{
    Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncAllResult(CancellationToken cancellationToken);

    Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncResult(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken);

    Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken);
}