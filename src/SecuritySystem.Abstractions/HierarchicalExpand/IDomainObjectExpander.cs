namespace SecuritySystem.HierarchicalExpand;

public interface IDomainObjectExpander<TDomainObject>
    where TDomainObject : class
{
    Task<IEnumerable<TDomainObject>> GetAllParents(IEnumerable<TDomainObject> startDomainObjects, CancellationToken cancellationToken);

    Task<IEnumerable<TDomainObject>> GetAllChildren(IEnumerable<TDomainObject> startDomainObjects, CancellationToken cancellationToken);
}