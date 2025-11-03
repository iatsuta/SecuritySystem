using CommonFramework;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.AncestorDenormalization;

public class DenormalizedAncestorsService<TDomainObject, TDirectAncestorLink>(
    IGenericRepository genericRepository,
    FullAncestorLinkInfo<TDomainObject, TDirectAncestorLink> fullAncestorLinkInfo,
    IAncestorLinkExtractor<TDomainObject, TDirectAncestorLink> ancestorLinkExtractor) : IDenormalizedAncestorsService<TDomainObject, TDirectAncestorLink>
    where TDirectAncestorLink : class, new()
    where TDomainObject : class
{
    private readonly Action<TDirectAncestorLink, TDomainObject> setFromAction =
        fullAncestorLinkInfo.Directed.FromPath.ToSetLambdaExpression().Compile();

    private readonly Action<TDirectAncestorLink, TDomainObject> setToAction =
        fullAncestorLinkInfo.Directed.ToPath.ToSetLambdaExpression().Compile();

    public async Task SyncUpAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncResult(domainObject, cancellationToken);

        await ApplySync(syncResult, cancellationToken);
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncAllResult(cancellationToken);

        await ApplySync(syncResult, cancellationToken);
    }

    public async Task SyncAsync(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncResult(updatedDomainObjectsBase, removedDomainObjects, cancellationToken);

        await ApplySync(syncResult, cancellationToken);
    }

    private async Task ApplySync(SyncResult<TDomainObject, TDirectAncestorLink> syncResult, CancellationToken cancellationToken)
    {
        foreach (var addLink in syncResult.Adding)
        {
            await SaveAncestor(CreateLink(addLink.Ancestor, addLink.Child), cancellationToken);
        }

        foreach (var removeLink in syncResult.Removing)
        {
            await RemoveAncestor(removeLink, cancellationToken);
        }
    }

    private async Task RemoveAncestor(TDirectAncestorLink domainObjectAncestorLink, CancellationToken cancellationToken)
    {
        await genericRepository.RemoveAsync(domainObjectAncestorLink, cancellationToken);
    }

    private async Task SaveAncestor(TDirectAncestorLink domainObjectAncestorLink, CancellationToken cancellationToken)
    {
        await genericRepository.SaveAsync(domainObjectAncestorLink, cancellationToken);
    }

    private TDirectAncestorLink CreateLink(TDomainObject ancestor, TDomainObject child)
    {
        var link = new TDirectAncestorLink();

        setFromAction(link, ancestor);
        setToAction(link, child);

        return link;
    }
}