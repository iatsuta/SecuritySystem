using CommonFramework;

using GenericQueryable;

using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class DomainObjectExpander<TDomainObject>(HierarchicalInfo<TDomainObject> hierarchicalInfo, IQueryableSource queryableSource)
	: IDomainObjectExpander<TDomainObject>
	where TDomainObject : class
{
	public async Task<IEnumerable<TDomainObject>> GetAllParents(IEnumerable<TDomainObject> startDomainObjects, CancellationToken cancellationToken)
	{
		var allResult = startDomainObjects.ToHashSet();

		for (var nextLayer = allResult; nextLayer.Any(); allResult.UnionWith(nextLayer))
		{
			nextLayer = await queryableSource.GetQueryable<TDomainObject>()
				.Where(currentObj => nextLayer.Contains(currentObj))
				.Select(hierarchicalInfo.ParentPath)
				.Where(nextObj => nextObj != null && !allResult.Contains(nextObj))
				.Select(nextObj => nextObj!)
				.GenericToHashSetAsync(cancellationToken);
		}

		return allResult;
	}

	public async Task<IEnumerable<TDomainObject>> GetAllChildren(IEnumerable<TDomainObject> startDomainObjects, CancellationToken cancellationToken)
	{
		var allResult = startDomainObjects.ToHashSet();

		for (var nextLayer = allResult; nextLayer.Any(); allResult.UnionWith(nextLayer))
		{
			nextLayer = await queryableSource.GetQueryable<TDomainObject>()
				.Where(hierarchicalInfo.ParentPath.Select(parentRef => parentRef != null && nextLayer.Contains(parentRef)))
				.Where(nextObj => !allResult.Contains(nextObj))
				.GenericToHashSetAsync(cancellationToken);
		}

		return allResult;
	}
}