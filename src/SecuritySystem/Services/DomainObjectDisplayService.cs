using System.Collections.Concurrent;

namespace SecuritySystem.Services;

public class DomainObjectDisplayService(IVisualIdentityInfoSource visualIdentityInfoSource, IEnumerable<DisplayObjectInfo> customDisplayObjectInfoList)
	: IDomainObjectDisplayService
{
	private readonly ConcurrentDictionary<Type, Delegate> cache = new();

	public string ToString<TDomainObject>(TDomainObject domainObject)
		where TDomainObject : class
	{
		var del = this.cache.GetOrAdd(typeof(TDomainObject), _ => this.GetActualDisplayObjectInfo<TDomainObject>().DisplayFunc);

		return ((Func<TDomainObject, string>)del).Invoke(domainObject);
	}

	private DisplayObjectInfo<TDomainObject> GetActualDisplayObjectInfo<TDomainObject>()
		where TDomainObject : class
	{
		if (customDisplayObjectInfoList.SingleOrDefault(info => info.DomainObjectType == typeof(TDomainObject)) is { } customInfo)
		{
			return (DisplayObjectInfo<TDomainObject>)customInfo;
		}
		else if (visualIdentityInfoSource.TryGetVisualIdentityInfo<TDomainObject>() is { } visualIdentityInfo)
		{
			return new DisplayObjectInfo<TDomainObject>(visualIdentityInfo.Name.Getter);
		}
		else
		{
			return DisplayObjectInfo<TDomainObject>.Default;
		}
	}
}