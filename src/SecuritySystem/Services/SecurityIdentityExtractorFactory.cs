using System.Collections.Concurrent;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.Services;

public class SecurityIdentityExtractorFactory(IServiceProvider serviceProvider) : ISecurityIdentityExtractorFactory
{
    private readonly ConcurrentDictionary<Type, object> cache = new();

    public ISecurityIdentityExtractor<TDomainObject> Create<TDomainObject>()
    {
        return (ISecurityIdentityExtractor<TDomainObject>)this.cache.GetOrAdd(
            typeof(TDomainObject),
            _ => serviceProvider.GetRequiredService<ISecurityIdentityExtractor<TDomainObject>>());
    }
}