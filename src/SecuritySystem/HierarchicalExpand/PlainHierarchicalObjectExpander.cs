using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public class PlainHierarchicalObjectExpander<TIdent> : IHierarchicalObjectExpander<TIdent>, IHierarchicalObjectQueryableExpander<TIdent>
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
}