using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public abstract record HierarchicalInfo<TDomainObject>(Expression<Func<TDomainObject, TDomainObject?>> ParentPath) : HierarchicalInfo
{
    public override Type DomainObjectType { get; } = typeof(TDomainObject);

    public Func<TDomainObject, TDomainObject?> ParentFunc { get; } = ParentPath.Compile();
}

public abstract record HierarchicalInfo
{
    public abstract Type DomainObjectType { get; }

    public abstract Type DirectedLinkType { get; }

    public abstract Type UndirectedLinkType { get; }
}