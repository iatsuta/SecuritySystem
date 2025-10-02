using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Services;

public class DenormalizedAncestorsServiceFactory(IServiceProvider serviceProvider) : IDenormalizedAncestorsServiceFactory
{
    public IDenormalizedAncestorsService<TDomainObject> Create<TDomainObject>()
    {
        var hierarchicalInfo = serviceProvider.GetRequiredService<HierarchicalInfo<TDomainObject>>();

        var denormalizedAncestorsServiceType =
            typeof(DenormalizedAncestorsService<,>).MakeGenericType(typeof(TDomainObject), hierarchicalInfo.DirectedLinkType);

        return (IDenormalizedAncestorsService<TDomainObject>)ActivatorUtilities.CreateInstance(serviceProvider, denormalizedAncestorsServiceType, hierarchicalInfo);
    }
}