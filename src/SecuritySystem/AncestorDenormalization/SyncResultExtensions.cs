namespace SecuritySystem.AncestorDenormalization;

public static class SyncResultExtensions
{
    public static SyncResult<TDomainObject, TDomainObjectAncestorLink> Aggregate<TDomainObject, TDomainObjectAncestorLink>(
        this IEnumerable<SyncResult<TDomainObject, TDomainObjectAncestorLink>> source)
    {
        return source.Aggregate(SyncResult<TDomainObject, TDomainObjectAncestorLink>.Empty, (s1, s2) => s1.Union(s2));
    }
}