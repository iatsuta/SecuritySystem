using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;
using SecuritySystem.Services;

namespace SecuritySystem.AncestorDenormalization;

public class DenormalizedAncestorsService<TDomainObject>(IServiceProvider serviceProvider, FullAncestorLinkInfo<TDomainObject> fullAncestorLinkInfo) : IDenormalizedAncestorsService<TDomainObject>
{
	private readonly IDenormalizedAncestorsService<TDomainObject> innerService =
		(IDenormalizedAncestorsService<TDomainObject>)ActivatorUtilities.CreateInstance(serviceProvider,
			typeof(DenormalizedAncestorsService<,>).MakeGenericType(typeof(TDomainObject), fullAncestorLinkInfo.DirectedLinkType));

	public Task SyncUpAsync(TDomainObject domainObject, CancellationToken cancellationToken) =>
		this.innerService.SyncUpAsync(domainObject, cancellationToken);

	public Task SyncAllAsync(CancellationToken cancellationToken) =>
		this.innerService.SyncAllAsync(cancellationToken);

	public Task SyncAsync(IEnumerable<TDomainObject> updatedDomainObjectsBase, IEnumerable<TDomainObject> removedDomainObjects, CancellationToken cancellationToken) =>
		this.innerService.SyncAsync(updatedDomainObjectsBase, removedDomainObjects, cancellationToken);
}

public class DenormalizedAncestorsService<TDomainObject, TDirectAncestorLink>(
    IGenericRepository genericRepository,
    FullAncestorLinkInfo<TDomainObject, TDirectAncestorLink> fullAncestorLinkInfo,
    IAncestorLinkExtractor<TDomainObject, TDirectAncestorLink> ancestorLinkExtractor) : IDenormalizedAncestorsService<TDomainObject>
    where TDirectAncestorLink : class, new()
    where TDomainObject : class
{
    public async Task SyncUpAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncResult(domainObject, cancellationToken);

        await this.ApplySync(syncResult, cancellationToken);
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncAllResult(cancellationToken);

        await this.ApplySync(syncResult, cancellationToken);
    }

    public async Task SyncAsync(
        IEnumerable<TDomainObject> updatedDomainObjectsBase,
        IEnumerable<TDomainObject> removedDomainObjects,
        CancellationToken cancellationToken)
    {
        var syncResult = await ancestorLinkExtractor.GetSyncResult(updatedDomainObjectsBase, removedDomainObjects, cancellationToken);

        await this.ApplySync(syncResult, cancellationToken);
    }

    private async Task ApplySync(SyncResult<TDomainObject, TDirectAncestorLink> syncResult, CancellationToken cancellationToken)
    {
        foreach (var addLink in syncResult.Adding)
        {
            await this.SaveAncestor(CreateLink(addLink.Ancestor, addLink.Child), cancellationToken);
        }

        foreach (var removeLink in syncResult.Removing)
        {
            await this.RemoveAncestor(removeLink, cancellationToken);
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

        fullAncestorLinkInfo.Directed.From.Setter(link, ancestor);
		fullAncestorLinkInfo.Directed.To.Setter(link, child);

        return link;
    }
}