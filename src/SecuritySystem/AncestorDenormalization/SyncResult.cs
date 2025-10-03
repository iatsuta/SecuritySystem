namespace SecuritySystem.AncestorDenormalization;

public record SyncResult<TDomainObject, TDomainObjectAncestorLink>(
    IEnumerable<AncestorLinkInfo<TDomainObject>> Adding,
    IEnumerable<TDomainObjectAncestorLink> Removing)
{
    public SyncResult<TDomainObject, TDomainObjectAncestorLink> Union(SyncResult<TDomainObject, TDomainObjectAncestorLink> other)
    {
        return new SyncResult<TDomainObject, TDomainObjectAncestorLink>(Adding.Union(other.Adding), Removing.Union(other.Removing));
    }

    public static SyncResult<TDomainObject, TDomainObjectAncestorLink> Empty { get; } = new([], []);
}