using System.Collections.Concurrent;

using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class DomainObjectIdentsParser(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : IDomainObjectIdentsParser
{
	private readonly ConcurrentDictionary<Type, IIdentsParser> parsersCache = new();

	public Array Parse(Type domainObjectType, IEnumerable<string> idents) =>
		parsersCache.GetOrAdd(domainObjectType, _ =>
			{
				var identityInfo = identityInfoSource.GetIdentityInfo(domainObjectType);

				return (IIdentsParser)serviceProvider.GetRequiredService(typeof(IdentsParser<>).MakeGenericType(identityInfo.IdentityType));
			})
			.Parse(idents);

}