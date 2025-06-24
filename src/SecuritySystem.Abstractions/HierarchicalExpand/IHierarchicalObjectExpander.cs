namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectExpander<TIdent>
    where TIdent : notnull
{
    /// <summary>
    /// Получение полного списка связанных идентефикаторов
    /// </summary>
    /// <param name="idents">Список базовых идентефикаторов</param>
    /// <param name="expandType">Тип разворачивания</param>
    /// <returns>HashSet/IQueryable></returns>
    IEnumerable<TIdent> Expand(IEnumerable<TIdent> idents, HierarchicalExpandType expandType);
}