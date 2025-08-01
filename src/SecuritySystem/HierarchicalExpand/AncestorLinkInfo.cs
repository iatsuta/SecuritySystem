using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public record AncestorLinkInfo<TDomainObject, TAncestorLink>(
    Expression<Func<TAncestorLink, TDomainObject>> FromPath,
    Expression<Func<TAncestorLink, TDomainObject>> ToPath)
{
    public AncestorLinkInfo<TDomainObject, TAncestorLink> Reverse() => new(this.ToPath, this.FromPath);

}