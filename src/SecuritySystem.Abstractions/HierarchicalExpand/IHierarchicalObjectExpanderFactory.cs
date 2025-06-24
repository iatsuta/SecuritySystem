namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectExpanderFactory<TIdent>
    where TIdent : notnull
{
    IHierarchicalObjectExpander<TIdent> Create(Type domainType);

    IHierarchicalObjectQueryableExpander<TIdent> CreateQuery(Type domainType);
}