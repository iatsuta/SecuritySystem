namespace SecuritySystem.AncestorDenormalization;

public static class SyncResultExtensions
{
    public static SyncResult<TDomainObject, TDirectAncestorLink> Aggregate<TDomainObject, TDirectAncestorLink>(
        this IEnumerable<SyncResult<TDomainObject, TDirectAncestorLink>> source)
    {
        return source.Aggregate(SyncResult<TDomainObject, TDirectAncestorLink>.Empty, (s1, s2) => s1.Union(s2));
    }
}