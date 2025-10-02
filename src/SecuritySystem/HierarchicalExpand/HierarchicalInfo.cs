using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public record HierarchicalInfo<TDomainObject, TDirectedLink, TUndirectedLink>(
    Expression<Func<TDomainObject, TDomainObject?>> ParentPath,
    AncestorLinkInfo<TDomainObject, TDirectedLink> DirectedAncestorLinkInfo,
    AncestorLinkInfo<TDomainObject, TUndirectedLink> UndirectedAncestorLinkInfo) : HierarchicalInfo<TDomainObject, TDirectedLink>(ParentPath, DirectedAncestorLinkInfo)
{
    public override Type UndirectedLinkType { get; } = typeof(TUndirectedLink);
}

public abstract record HierarchicalInfo<TDomainObject, TDirectedLink>(
    Expression<Func<TDomainObject, TDomainObject?>> ParentPath,
    AncestorLinkInfo<TDomainObject, TDirectedLink> DirectedAncestorLinkInfo) : HierarchicalInfo<TDomainObject>(ParentPath)
{
    public override Type DirectedLinkType { get; } = typeof(TDirectedLink);
}

public abstract record HierarchicalInfo<TDomainObject>(Expression<Func<TDomainObject, TDomainObject?>> ParentPath) : HierarchicalInfo
{
    //public override Type DomainObjectType { get; } = domainType;

    public Func<TDomainObject, TDomainObject?> ParentFunc { get; } = ParentPath.Compile();
}

public abstract record HierarchicalInfo
{
    //public abstract Type DomainObjectType { get; }

    public abstract Type DirectedLinkType { get; }

    public abstract Type UndirectedLinkType { get; }
}