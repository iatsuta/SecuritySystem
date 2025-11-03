using CommonFramework.DictionaryCache;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectExpanderFactory : IHierarchicalObjectExpanderFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IIdentityInfoSource identityInfoSource;
    private readonly IRealTypeResolver realTypeResolver;
    private readonly IDictionaryCache<Type, IHierarchicalObjectExpander> cache;

    public HierarchicalObjectExpanderFactory(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource, IRealTypeResolver realTypeResolver)
    {
        this.serviceProvider = serviceProvider;
        this.identityInfoSource = identityInfoSource;
        this.realTypeResolver = realTypeResolver;

        this.cache = new DictionaryCache<Type, IHierarchicalObjectExpander>(this.CreateInternal);
    }

    private IHierarchicalObjectExpander CreateInternal(Type domainType)
    {
        var realType = realTypeResolver.Resolve(domainType);

        if (realType != domainType)
        {
            return this.cache[realType];
        }
        else
        {
            var fullAncestorLinkInfo = (FullAncestorLinkInfo?)serviceProvider.GetService(typeof(FullAncestorLinkInfo<>).MakeGenericType(domainType));

            var identityInfo = identityInfoSource.GetIdentityInfo(domainType);

            if (fullAncestorLinkInfo != null)
            {
                var expanderType = typeof(HierarchicalObjectAncestorLinkExpander<,,,>)
                    .MakeGenericType(domainType, fullAncestorLinkInfo.DirectedLinkType, fullAncestorLinkInfo.UndirectedLinkType, identityInfo.IdentityType);

                return (IHierarchicalObjectExpander)ActivatorUtilities.CreateInstance(serviceProvider, expanderType, fullAncestorLinkInfo, identityInfo);

            }
            else
            {
                var expanderType = typeof(PlainHierarchicalObjectExpander<>).MakeGenericType(identityInfo.IdentityType);

                return (IHierarchicalObjectExpander)ActivatorUtilities.CreateInstance(serviceProvider, expanderType);
            }
        }
    }

    public IHierarchicalObjectExpander Create(Type domainType)
    {
        return this.cache[domainType];
    }
}