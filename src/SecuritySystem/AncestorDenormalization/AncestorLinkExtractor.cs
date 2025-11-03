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

    private readonly FetchRule<TDirectAncestorLink> linkFetchRule =
        FetchRule<TDirectAncestorLink>.Create(fullAncestorLinkInfo.Directed.FromPath).Fetch(fullAncestorLinkInfo.Directed.ToPath);

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncAllResult(CancellationToken cancellationToken)
    {
        var existsDomainObjects = await queryableSource.GetQueryable<TDomainObject>().GenericToListAsync(cancellationToken);

        var existsLinks = await queryableSource.GetQueryable<TDirectAncestorLink>().WithFetch(linkFetchRule).GenericToListAsync(cancellationToken);

        var nonExistsDomainObjects = existsLinks.Select(this.ToInfo).SelectMany(link => new[] { link.Ancestor, link.Child }).Except(existsDomainObjects);

        return await this.GetSyncResult(existsDomainObjects, nonExistsDomainObjects, cancellationToken);
    }

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken)
    {
        var existsLinkInfos = await updatedDomainObjectsBase.SyncWhenAll(domainObject => this.GetSyncResult(domainObject, cancellationToken));

        var forceRemovedLinks = await this.GetExistsLinks(removedDomainObjects, cancellationToken);

        var forceRemovedLinksSyncResult = new SyncResult<TDomainObject, TDirectAncestorLink>([], forceRemovedLinks);

        return existsLinkInfos.Union([forceRemovedLinksSyncResult]).Aggregate();
    }

    public async Task<SyncResult<TDomainObject, TDirectAncestorLink>> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var existsLinks = await this.GetExistsLinks([domainObject], cancellationToken);

        var expectedLinks = await this.GetExpectedAncestorLinks(domainObject, cancellationToken);

        var mergeResult = existsLinks.GetMergeResult(expectedLinks, ToInfo, v => v);

        return new SyncResult<TDomainObject, TDirectAncestorLink>(mergeResult.AddingItems, mergeResult.RemovingItems);
    }

    private async Task<IEnumerable<TDirectAncestorLink>> GetExistsLinks(IEnumerable<TDomainObject> domainObjects, CancellationToken cancellationToken)
    {
        var filter = ancestorLinkInfo.FromPath.Select(fromObj => domainObjects.Contains(fromObj))
            .BuildOr(ancestorLinkInfo.ToPath.Select(toObj => domainObjects.Contains(toObj)));

        return await queryableSource.GetQueryable<TDirectAncestorLink>().Where(filter).WithFetch(linkFetchRule).GenericToListAsync(cancellationToken);
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