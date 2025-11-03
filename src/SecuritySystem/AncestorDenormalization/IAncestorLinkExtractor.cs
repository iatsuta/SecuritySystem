namespace SecuritySystem.AncestorDenormalization;

public interface IAncestorLinkExtractor<TDomainObject, TDirectAncestorLink>
{
    Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncAllResult(CancellationToken cancellationToken);

    Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken);

    Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken);
}