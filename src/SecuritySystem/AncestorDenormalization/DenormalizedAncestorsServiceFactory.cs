using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.AncestorDenormalization;

public class DenormalizedAncestorsServiceFactory(IServiceProvider serviceProvider) : IDenormalizedAncestorsServiceFactory
{
    public IDenormalizedAncestorsService<TDomainObject> Create<TDomainObject>()
    {
        var hierarchicalInfo = serviceProvider.GetRequiredService<HierarchicalInfo<TDomainObject>>();

        var generics = new[] { typeof(TDomainObject), hierarchicalInfo.DirectedLinkType };

        return (IDenormalizedAncestorsService<TDomainObject>)
            serviceProvider.GetRequiredService(typeof(IDenormalizedAncestorsService<,>).MakeGenericType(generics));
    }
}