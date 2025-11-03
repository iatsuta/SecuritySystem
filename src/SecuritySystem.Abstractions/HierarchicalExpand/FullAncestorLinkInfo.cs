namespace SecuritySystem.HierarchicalExpand;

public record FullAncestorLinkInfo<TDomainObject, TDirectedLink, TUndirectedLink>(
    AncestorLinkInfo<TDomainObject, TDirectedLink> Directed,
    AncestorLinkInfo<TDomainObject, TUndirectedLink> Undirected) : FullAncestorLinkInfo<TDomainObject, TDirectedLink>(Directed)
{
    public override Type UndirectedLinkType { get; } = typeof(TUndirectedLink);
}

public abstract record FullAncestorLinkInfo<TDomainObject, TDirectedLink>(AncestorLinkInfo<TDomainObject, TDirectedLink> Directed) : FullAncestorLinkInfo<TDomainObject>
{
    public override Type DirectedLinkType { get; } = typeof(TDirectedLink);
}

public abstract record FullAncestorLinkInfo<TDomainObject> : FullAncestorLinkInfo
{
    public override Type DomainObjectType { get; } = typeof(TDomainObject);
}

public abstract record FullAncestorLinkInfo
{
    public abstract Type DomainObjectType { get; }

    public abstract Type DirectedLinkType { get; }

    public abstract Type UndirectedLinkType { get; }
}