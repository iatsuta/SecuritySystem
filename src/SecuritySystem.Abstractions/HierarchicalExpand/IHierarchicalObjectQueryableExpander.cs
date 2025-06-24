using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectQueryableExpander<TIdent>
    where TIdent : notnull
{
    Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression(HierarchicalExpandType expandType);

    Expression<Func<TIdent, IEnumerable<TIdent>>>? TryGetSingleExpandExpression(HierarchicalExpandType expandType);
}