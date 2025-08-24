using System.Linq.Expressions;

namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectExpander<TIdent> : IHierarchicalObjectExpander
    where TIdent : notnull
{
    /// <summary>
    /// Получение полного списка связанных идентефикаторов
    /// </summary>
    /// <param name="idents">Список базовых идентефикаторов</param>
    /// <param name="expandType">Тип разворачивания</param>
    /// <returns>HashSet/IQueryable></returns>
    IEnumerable<TIdent> Expand(IEnumerable<TIdent> idents, HierarchicalExpandType expandType);
    
    Expression<Func<IEnumerable<TIdent>, IEnumerable<TIdent>>> GetExpandExpression(HierarchicalExpandType expandType);

    Expression<Func<TIdent, IEnumerable<TIdent>>>? TryGetSingleExpandExpression(HierarchicalExpandType expandType);
}

public interface IHierarchicalObjectExpander
{
    Array Expand(Array idents, HierarchicalExpandType expandType);
}