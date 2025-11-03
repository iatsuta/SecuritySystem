using CommonFramework;

using GenericQueryable;
using GenericQueryable.Fetching;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.AncestorDenormalization;

public class AncestorLinkExtractor<TDomainObject, TDirectAncestorLink>(
    IQueryableSource queryableSource,
    IDomainObjectExpander<TDomainObject> domainObjectExpander,
    FullAncestorLinkInfo<TDomainObject, TDirectAncestorLink> fullAncestorLinkInfo)
    : IAncestorLinkExtractor<TDomainObject, TDirectAncestorLink>
    where TDirectAncestorLink : class
    where TDomainObject : class
{
    private readonly AncestorLinkInfo<TDomainObject, TDirectAncestorLink> ancestorLinkInfo = fullAncestorLinkInfo.Directed;

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncAllResult(CancellationToken cancellationToken)
    {
        var existsDomainObjects = await queryableSource.GetQueryable<TDomainObject>().GenericToListAsync(cancellationToken);

        var existsLinks = await queryableSource.GetQueryable<TDirectAncestorLink>().GenericToListAsync(cancellationToken);

        var nonExistsDomainObjects = existsLinks
            .SelectMany(link => new[] { ancestorLinkInfo.FromFunc(link), ancestorLinkInfo.ToFunc(link) })
            .Except(existsDomainObjects);

        return await this.GetSyncResult(existsDomainObjects, nonExistsDomainObjects, cancellationToken);
    }

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken)
    {
        var existsLinkInfos = await updatedDomainObjectsBase.SyncWhenAll(domainObject => this.GetSyncResult(domainObject, cancellationToken));

        var removedLinks = await removedDomainObjects.SyncWhenAll(domainObject => this.GetExistsLinks(domainObject, cancellationToken));

        var removedLinkInfos = removedLinks.Select(links => new SyncResult<TDomainObject, TDirectAncestorLink>([], links));

        return existsLinkInfos.Union(removedLinkInfos).Aggregate();
    }

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var existsLinks = await this.GetExistsLinks(domainObject, cancellationToken);

        var expectedLinks = await this.GetExpectedAncestorLinks(domainObject, cancellationToken);

        var mergeResult = existsLinks.GetMergeResult(expectedLinks, ToInfo, v => v);

        return new SyncResult<TDomainObject, TDirectAncestorLink>(mergeResult.AddingItems, mergeResult.RemovingItems);
    }

    private async Task<IEnumerable<TDirectAncestorLink>> GetExistsLinks(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var filter = ancestorLinkInfo.FromPath.Select(fromObj => fromObj == domainObject)
            .BuildOr(ancestorLinkInfo.ToPath.Select(toObj => toObj == domainObject));

        return await
            queryableSource
                .GetQueryable<TDirectAncestorLink>()
                .Where(filter)
                .WithFetch(r => r.Fetch(ancestorLinkInfo.FromPath).Fetch(ancestorLinkInfo.ToPath))
                .GenericToListAsync(cancellationToken);
    }

    private async Task<IEnumerable<AncestorLinkInfo<TDomainObject>>> GetExpectedAncestorLinks(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var parents = await domainObjectExpander.GetAllParents([domainObject], cancellationToken);

        var children = await domainObjectExpander.GetAllChildren([domainObject], cancellationToken);

        var parentsLinks = parents.Select(parent => new AncestorLinkInfo<TDomainObject>(parent, domainObject));

        var childrenLinks = children.Select(child => new AncestorLinkInfo<TDomainObject>(domainObject, child));

        return parentsLinks.Union(childrenLinks);
    }

    private AncestorLinkInfo<TDomainObject> ToInfo(TDirectAncestorLink link)
    {
        return new AncestorLinkInfo<TDomainObject>(
            ancestorLinkInfo.FromFunc(link),
            ancestorLinkInfo.ToFunc(link));
    }
}