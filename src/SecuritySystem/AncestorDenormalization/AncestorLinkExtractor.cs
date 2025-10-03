using CommonFramework;

using GenericQueryable;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.AncestorDenormalization;

public class AncestorLinkExtractor<TDomainObject, TDomainObjectAncestorLink>(
    IQueryableSource queryableSource,
    HierarchicalInfo<TDomainObject, TDomainObjectAncestorLink> hierarchicalInfo)
    : IAncestorLinkExtractor<TDomainObject, TDomainObjectAncestorLink>
    where TDomainObjectAncestorLink : class
    where TDomainObject : class
{
    public async Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncAllResult(CancellationToken cancellationToken)
    {
        var existsDomainObjects = await queryableSource.GetQueryable<TDomainObject>().GenericToListAsync(cancellationToken);

        var existsLinks = await queryableSource.GetQueryable<TDomainObjectAncestorLink>().GenericToListAsync(cancellationToken);

        var nonExistsDomainObjects = existsLinks
            .SelectMany(link => new[] { hierarchicalInfo.DirectedAncestorLinkInfo.FromFunc(link), hierarchicalInfo.DirectedAncestorLinkInfo.ToFunc(link) })
            .Except(existsDomainObjects);

        return await GetSyncResult(existsDomainObjects, nonExistsDomainObjects, cancellationToken);
    }

    public async Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncResult(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken)
    {
        var existsLinkInfos = await updatedDomainObjectsBase.SyncWhenAll(domainObject => this.GetSyncResult(domainObject, cancellationToken));

        var removedLinks = await removedDomainObjects.SyncWhenAll(domainObject => this.GetExistsLinks(domainObject, cancellationToken));

        var removedLinkInfos = removedLinks.Select(links => new SyncResult<TDomainObject, TDomainObjectAncestorLink>([], links));

        return existsLinkInfos.Union(removedLinkInfos).Aggregate();
    }

    public async Task<SyncResult<TDomainObject, TDomainObjectAncestorLink>> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var existsLinks = await this.GetExistsLinks(domainObject, cancellationToken);

        var expectedLinks = await this.GetExpectedAncestorLinks(domainObject, cancellationToken);

        var mergeResult = existsLinks.GetMergeResult(expectedLinks, ToInfo, v => v);

        return new SyncResult<TDomainObject, TDomainObjectAncestorLink>(mergeResult.AddingItems, mergeResult.RemovingItems);
    }

    private async Task<IEnumerable<TDomainObjectAncestorLink>> GetExistsLinks(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var fromAncestors = await
            queryableSource
                .GetQueryable<TDomainObjectAncestorLink>()
                .Where(hierarchicalInfo.DirectedAncestorLinkInfo.FromPath.Select(fromObj => fromObj == domainObject))
                .GenericToListAsync(cancellationToken);

        var toAncestors = await
            queryableSource
                .GetQueryable<TDomainObjectAncestorLink>()
                .Where(hierarchicalInfo.DirectedAncestorLinkInfo.ToPath.Select(toObj => toObj == domainObject))
                .GenericToListAsync(cancellationToken);

        return fromAncestors.Union(toAncestors);
    }

    private async Task<IEnumerable<AncestorLinkInfo<TDomainObject>>> GetExpectedAncestorLinks(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var parents = domainObject.GetAllElements(hierarchicalInfo.ParentFunc);

        var children = await this.GetAllChildren(domainObject, cancellationToken);

        var parentsLinks = parents.Select(parent => new AncestorLinkInfo<TDomainObject>(parent, domainObject));

        var childrenLinks = children.Select(child => new AncestorLinkInfo<TDomainObject>(domainObject, child));

        return parentsLinks.Union(childrenLinks);
    }

    private async Task<IEnumerable<TDomainObject>> GetAllChildren(TDomainObject startDomainObject, CancellationToken cancellationToken)
    {
        var allResult = new List<TDomainObject>();

        for (var next = new List<TDomainObject> { startDomainObject }; next.Any(); allResult.AddRange(next))
        {
            next = await queryableSource.GetQueryable<TDomainObject>()
                .Where(hierarchicalInfo.ParentPath.Select(parentRef => parentRef != null && next.Contains(parentRef)))
                .GenericToListAsync(cancellationToken);
        }

        return allResult;
    }

    private AncestorLinkInfo<TDomainObject> ToInfo(TDomainObjectAncestorLink link)
    {
        return new AncestorLinkInfo<TDomainObject>(
            hierarchicalInfo.DirectedAncestorLinkInfo.FromFunc(link),
            hierarchicalInfo.DirectedAncestorLinkInfo.ToFunc(link));
    }
}