using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public class PlainHierarchicalObjectExpander<TIdent> : IHierarchicalObjectExpander<TIdent>
    where TIdent : notnull
{
    public IEnumerable<TIdent> Expand(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return idents;
    }

    public Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression(HierarchicalExpandType expandType)
    {
        return v => v;
    }

    public Expression<Func<TIdent, IEnumerable<TIdent>>>? TryGetSingleExpandExpression(HierarchicalExpandType expandType)
    {
        return null;
    }

    public Array Expand(Array idents, HierarchicalExpandType expandType)
    {
        return idents;
    }

    public Dictionary<TIdent, TIdent> ExpandWithParents(IEnumerable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents, expandType);
    }

    public Dictionary<TIdent, TIdent> ExpandWithParents(IQueryable<TIdent> idents, HierarchicalExpandType expandType)
    {
        return this.ExpandWithParentsImplementation(idents, expandType);
    }

    public Dictionary<TIdent, TIdent> ExpandWithParentsImplementation(IEnumerable<TIdent> idents, HierarchicalExpandType _)
    {
        return idents.ToDictionary(id => id, _ => default(TIdent)!);
    }
}