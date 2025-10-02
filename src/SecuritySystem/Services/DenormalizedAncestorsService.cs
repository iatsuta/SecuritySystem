using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using GenericQueryable;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Services;

public class DenormalizedAncestorsService<TDomainObject, TDomainObjectAncestorLink>(
    IQueryableSource queryableSource,
    IGenericRepository genericRepository,
    HierarchicalInfo<TDomainObject, TDomainObjectAncestorLink> hierarchicalInfo) : IDenormalizedAncestorsService<TDomainObject>
    where TDomainObjectAncestorLink : class, new()
    where TDomainObject : class
{
    private readonly Action<TDomainObjectAncestorLink, TDomainObject> setFromAction =
        hierarchicalInfo.DirectedAncestorLinkInfo.FromPath.ToSetLambdaExpression().Compile();

    private readonly Action<TDomainObjectAncestorLink, TDomainObject> setToAction =
        hierarchicalInfo.DirectedAncestorLinkInfo.ToPath.ToSetLambdaExpression().Compile();

    public async Task SyncUpAsync(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var ancestorDiffLinks = await this.GetAncestorDifference(domainObject, cancellationToken);

        Func<DiffAncestorLinks, MergeResult<TDomainObjectAncestorLink, AncestorLink>> getMergeFunc = diff =>
            diff.PersistentLinks.GetMergeResult(
                diff.ActualLinks,
                z => new AncestorLink
                {
                    Ancestor = hierarchicalInfo.DirectedAncestorLinkInfo.FromFunc(z),
                    Child = hierarchicalInfo.DirectedAncestorLinkInfo.ToFunc(z)
                },
                z => z);
        
        var ancestorMergeResult = getMergeFunc(ancestorDiffLinks);

        var addedLinks = ancestorMergeResult
            .AddingItems
            .Select(z => this.CreateLink(z.Ancestor, z.Child));

        foreach (var removeLink in ancestorMergeResult.RemovingItems)
        {
            await this.RemoveAncestor(removeLink, cancellationToken);
        }

        foreach (var addLink in addedLinks)
        {
            await this.SaveAncestor(addLink, cancellationToken);
        }
    }

    public async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        foreach (var domainObject in await queryableSource.GetQueryable<TDomainObject>().GenericToListAsync(cancellationToken))
        {
            await this.SyncUpAsync(domainObject, cancellationToken);
        }
    }

    public async Task SyncAsync(IEnumerable<TDomainObject> updatedDomainObjectsBase, IEnumerable<TDomainObject> removedDomainObjects, CancellationToken cancellationToken)
    {
        var updatedDomainObjects = updatedDomainObjectsBase.ToList();

        var fromSyncResult = await Task.WhenAll(updatedDomainObjects.Select(v => this.GetSyncResult(v, cancellationToken)));

        var fromUnSyncResult = await this.GetUnSyncResult(removedDomainObjects, cancellationToken);

        var combine = fromSyncResult.Concat([fromUnSyncResult]).Aggregate((prev, current) => prev.Union(current));

        var newAncestorLinks = combine.Adding.Select(z => this.CreateLink(z.Ancestor, z.Child)).ToList();

        foreach (var addLink in newAncestorLinks)
        {
            await this.SaveAncestor(addLink, cancellationToken);
        }

        foreach (var removeLink in combine.Removing)
        {
            await this.RemoveAncestor(removeLink, cancellationToken);
        }
    }

    private async Task<SyncResult> GetUnSyncResult(IEnumerable<TDomainObject> domainObjects, CancellationToken cancellationToken)
    {
        var cachedDomainObject = domainObjects.ToList();

        var filter =

            ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObjectAncestorLink, bool>>(ee =>

                link =>
                    cachedDomainObject.Contains(ee.Evaluate(hierarchicalInfo.DirectedAncestorLinkInfo.FromPath, link))
                    || cachedDomainObject.Contains(ee.Evaluate(hierarchicalInfo.DirectedAncestorLinkInfo.ToPath, link)));

        var removingAncestors = await queryableSource
            .GetQueryable<TDomainObjectAncestorLink>()
            .Where(filter)
            .GenericToListAsync(cancellationToken);

        return new SyncResult([], removingAncestors);
    }

    private async Task<SyncResult> GetSyncResult(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var children = await this.GetAllChildren(domainObject, cancellationToken);

        var ancestorDiffs = await Task.WhenAll(children.Select(c => this.GetAncestorDifference(c, cancellationToken)));

        Func<DiffAncestorLinks, MergeResult<TDomainObjectAncestorLink, AncestorLink>> getMergeFunc = diff =>
            diff.PersistentLinks.GetMergeResult(
                diff.ActualLinks,
                z => new AncestorLink
                {
                    Ancestor = hierarchicalInfo.DirectedAncestorLinkInfo.FromFunc(z),
                    Child = hierarchicalInfo.DirectedAncestorLinkInfo.ToFunc(z)
                },
                z => z);

        var mergeResults = ancestorDiffs.Select(getMergeFunc).ToList();

        if (!mergeResults.Any())
        {
            return new SyncResult();
        }

        var addedLinks = mergeResults.Any(q => q.AddingItems.Any())
                             ? mergeResults.Select(IEnumerable<AncestorLink> (z) => z.AddingItems)
                                           .Aggregate((prev, current) => prev.Concat(current))
                             : [];

        var removingLinks = mergeResults.Any(q => q.RemovingItems.Any())
                                ? mergeResults.Select(IEnumerable<TDomainObjectAncestorLink> (z) => z.RemovingItems)
                                              .Aggregate((prev, current) => prev.Concat(current))
                                : [];


        return new SyncResult(addedLinks, removingLinks);
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

    private async Task<DiffAncestorLinks> GetAncestorDifference(TDomainObject domainObject, CancellationToken cancellationToken)
    {
        var actualAncestors = domainObject
            .GetAllElements(hierarchicalInfo.ParentFunc)
            .Select(z => new AncestorLink { Child = domainObject, Ancestor = z })
            .ToList();
        
        var persistentAncestors = await 
            queryableSource
            .GetQueryable<TDomainObjectAncestorLink>()
            .Where(hierarchicalInfo.DirectedAncestorLinkInfo.ToPath.Select(toObj => toObj == domainObject))
            .GenericToListAsync(cancellationToken);

        return new DiffAncestorLinks(actualAncestors, persistentAncestors);
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

        this.setFromAction(link, ancestor);
        this.setToAction(link, child);

        return link;
    }

    private readonly record struct AncestorLink
    { 
        public required TDomainObject Ancestor { get; init; }
        
        public required TDomainObject Child { get; init; }
    };

    private readonly record struct DiffAncestorLinks(
        IEnumerable<AncestorLink> ActualLinks,
        IEnumerable<TDomainObjectAncestorLink> PersistentLinks);

    private readonly record struct SyncResult(IEnumerable<AncestorLink> Adding, IEnumerable<TDomainObjectAncestorLink> Removing)
    {
        public SyncResult Union(SyncResult other)
        {
            return new SyncResult(this.Adding.Union(other.Adding), this.Removing.Union(other.Removing));
        }
    }
}
