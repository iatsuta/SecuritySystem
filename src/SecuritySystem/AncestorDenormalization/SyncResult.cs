using CommonFramework;

namespace SecuritySystem.AncestorDenormalization;

public record SyncResult<TDomainObject, TDirectAncestorLink>(
    DeepEqualsCollection<AncestorLinkInfo<TDomainObject>> Adding,
    DeepEqualsCollection<TDirectAncestorLink> Removing)
{
    public SyncResult(IEnumerable<AncestorLinkInfo<TDomainObject>> adding,
        IEnumerable<TDirectAncestorLink> removing) :
        this(DeepEqualsCollection.Create(adding), DeepEqualsCollection.Create(removing))
    {
    }

    public SyncResult<TDomainObject, TDirectAncestorLink> Union(SyncResult<TDomainObject, TDirectAncestorLink> other)
    {
        return new SyncResult<TDomainObject, TDirectAncestorLink>(this.Adding.Union(other.Adding).ToArray(), this.Removing.Union(other.Removing).ToArray());
    }

    public static SyncResult<TDomainObject, TDirectAncestorLink> Empty { get; } = new([], []);
}