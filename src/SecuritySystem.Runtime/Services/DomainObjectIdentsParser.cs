using System.Collections.Concurrent;

using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class DomainObjectIdentsParser<TSource>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource) : IDomainObjectIdentsParser<TSource>
{
    private readonly ConcurrentDictionary<Type, IIdentsParser<TSource>> parsersCache = new();

    public Array Parse(Type domainObjectType, IEnumerable<TSource> idents) =>
        parsersCache.GetOrAdd(domainObjectType, _ =>
            {
                var identityInfo = identityInfoSource.GetIdentityInfo(domainObjectType);

                return (IIdentsParser<TSource>)serviceProvider.GetRequiredService(
                    typeof(IIdentsParser<,>).MakeGenericType(typeof(TSource), identityInfo.IdentityType));
            })
            .Parse(idents);

}