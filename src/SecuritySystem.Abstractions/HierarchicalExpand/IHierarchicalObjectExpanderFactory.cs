namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectExpanderFactory
{
    IHierarchicalObjectExpander<TIdent> Create<TIdent>(Type domainType)
        where TIdent : notnull;

    IHierarchicalObjectQueryableExpander<TIdent> CreateQuery<TIdent>(Type domainType)
        where TIdent : notnull;
}