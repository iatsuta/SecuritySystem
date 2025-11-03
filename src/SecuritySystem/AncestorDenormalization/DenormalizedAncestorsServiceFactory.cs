using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.AncestorDenormalization;

public class DenormalizedAncestorsServiceFactory(IServiceProvider serviceProvider) : IDenormalizedAncestorsServiceFactory
{
    public IDenormalizedAncestorsService<TDomainObject> Create<TDomainObject>()
    {
        var fullAncestorLinkInfo = serviceProvider.GetRequiredService<FullAncestorLinkInfo<TDomainObject>>();

        var generics = new[] { typeof(TDomainObject), fullAncestorLinkInfo.DirectedLinkType };

        return (IDenormalizedAncestorsService<TDomainObject>)
            serviceProvider.GetRequiredService(typeof(IDenormalizedAncestorsService<,>).MakeGenericType(generics));
    }
}