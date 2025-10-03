using CommonFramework;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.AncestorDenormalization;

public class DenormalizedAncestorsService<TDomainObject, TDomainObjectAncestorLink>(
    IGenericRepository genericRepository,
    HierarchicalInfo<TDomainObject, TDomainObjectAncestorLink> hierarchicalInfo,
    IAncestorLinkExtractor<TDomainObject, TDomainObjectAncestorLink> ancestorLinkExtractor) : IDenormalizedAncestorsService<TDomainObject, TDomainObjectAncestorLink>
    where TDomainObjectAncestorLink : class, new()
    where TDomainObject : class
{
    private readonly Action<TDomainObjectAncestorLink, TDomainObject> setFromAction =
        hierarchicalInfo.DirectedAncestorLinkInfo.FromPath.ToSetLambdaExpression().Compile();

    private readonly Action<TDomainObjectAncestorLink, TDomainObject> setToAction =
        hierarchicalInfo.DirectedAncestorLinkInfo.ToPath.ToSetLambdaExpression().Compile();

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

    private async Task ApplySync(SyncResult<TDomainObject, TDomainObjectAncestorLink> syncResult, CancellationToken cancellationToken)
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

    private async Task RemoveAncestor(TDomainObjectAncestorLink domainObjectAncestorLink, CancellationToken cancellationToken)
    {
        await genericRepository.RemoveAsync(domainObjectAncestorLink, cancellationToken);
    }

    private async Task SaveAncestor(TDomainObjectAncestorLink domainObjectAncestorLink, CancellationToken cancellationToken)
    {
        await genericRepository.SaveAsync(domainObjectAncestorLink, cancellationToken);
    }

    private TDomainObjectAncestorLink CreateLink(TDomainObject ancestor, TDomainObject child)
    {
        var link = new TDomainObjectAncestorLink();

        setFromAction(link, ancestor);
        setToAction(link, child);

        return link;
    }
}