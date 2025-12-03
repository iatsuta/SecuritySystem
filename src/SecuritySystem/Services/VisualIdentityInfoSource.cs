using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.DictionaryCache;

namespace SecuritySystem.Services;

public class VisualIdentityInfoSource(IVisualIdentityPropertyExtractor propertyExtractor, IEnumerable<VisualIdentityInfo> customInfoList)
	: IVisualIdentityInfoSource
{
	private readonly IDictionaryCache<Type, VisualIdentityInfo?> cache = new DictionaryCache<Type, VisualIdentityInfo?>(domainType =>
	{
		var customInfo = customInfoList.SingleOrDefault(identityInfo => identityInfo.DomainObjectType == domainType);

		if (customInfo != null)
		{
			return customInfo;
		}
		else
		{
			var nameProperty = propertyExtractor.TryExtract(domainType);

			if (nameProperty == null)
			{
				return null;
			}
			else
			{
				var idPath = nameProperty.ToLambdaExpression();

				return new Func<Expression<Func<object, string>>, VisualIdentityInfo<object>>(CreateVisualIdentityInfo)
					.CreateGenericMethod(domainType)
					.Invoke<VisualIdentityInfo>(null, idPath);
			}
		}

	}).WithLock();


	public VisualIdentityInfo<TDomainObject>? TryGetVisualIdentityInfo<TDomainObject>()
	{
		return (VisualIdentityInfo<TDomainObject>?)this.cache[typeof(TDomainObject)];
	}

	public VisualIdentityInfo<TDomainObject> GetVisualIdentityInfo<TDomainObject>()
	{
		return this.TryGetVisualIdentityInfo<TDomainObject>() ??
		       throw new Exception($"{nameof(VisualIdentityInfo)} for {typeof(TDomainObject).Name} not found");
	}

	private static VisualIdentityInfo<TDomainObject> CreateVisualIdentityInfo<TDomainObject>(Expression<Func<TDomainObject, string>> namePath)
	{
		return new VisualIdentityInfo<TDomainObject>(namePath);
	}
}