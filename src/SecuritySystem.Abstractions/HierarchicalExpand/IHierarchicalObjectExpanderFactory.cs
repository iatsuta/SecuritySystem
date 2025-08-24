namespace SecuritySystem.HierarchicalExpand;

public interface IHierarchicalObjectExpanderFactory
{
    IHierarchicalObjectExpander Create(Type domainType);

    IHierarchicalObjectExpander<TIdent> Create<TIdent>(Type domainType)
        where TIdent : notnull;
}