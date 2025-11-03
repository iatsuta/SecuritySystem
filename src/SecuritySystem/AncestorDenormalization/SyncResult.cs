namespace SecuritySystem.AncestorDenormalization;

public record SyncResult<TDomainObject, TDirectAncestorLink>(
    IEnumerable<AncestorLinkInfo<TDomainObject>> Adding,
    IEnumerable<TDirectAncestorLink> Removing)
{
    public SyncResult<TDomainObject, TDirectAncestorLink> Union(SyncResult<TDomainObject, TDirectAncestorLink> other)
    {
        return new SyncResult<TDomainObject, TDirectAncestorLink>(Adding.Union(other.Adding), Removing.Union(other.Removing));
    }

    public static SyncResult<TDomainObject, TDirectAncestorLink> Empty { get; } = new([], []);
}