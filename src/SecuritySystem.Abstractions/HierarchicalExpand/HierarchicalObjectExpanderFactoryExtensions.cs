namespace SecuritySystem.HierarchicalExpand;

public static class HierarchicalObjectExpanderFactoryExtensions
{
    public static IHierarchicalObjectExpander<TIdent> Create<TIdent>(
        this IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory, Type domainType)
        where TIdent : notnull
    {
        return (IHierarchicalObjectExpander<TIdent>)hierarchicalObjectExpanderFactory.Create(domainType);
    }
}