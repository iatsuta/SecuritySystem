using CommonFramework.DictionaryCache;

using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectExpanderFactory : IHierarchicalObjectExpanderFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IIdentityInfoSource identityInfoSource;
    private readonly IRealTypeResolver? realTypeResolver;
    private readonly IDictionaryCache<Type, IHierarchicalObjectExpander> cache;

    public HierarchicalObjectExpanderFactory(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource, IRealTypeResolver? realTypeResolver = null)
    {
        this.serviceProvider = serviceProvider;
        this.identityInfoSource = identityInfoSource;
        this.realTypeResolver = realTypeResolver;

        this.cache = new DictionaryCache<Type, IHierarchicalObjectExpander>(this.CreateInternal);
    }

    private IHierarchicalObjectExpander CreateInternal(Type domainType)
    {
        var realType = realTypeResolver?.Resolve(domainType) ?? domainType;

        if (realType != domainType)
        {
            return this.cache[realType];
        }
        else
        {
            var hierarchicalInfo = (HierarchicalInfo?)serviceProvider.GetService(typeof(HierarchicalInfo<>).MakeGenericType(domainType));

            var identityInfo = identityInfoSource.GetIdentityInfo(domainType);

            if (hierarchicalInfo != null)
            {
                var expanderType = typeof(HierarchicalObjectAncestorLinkExpander<,,,>)
                    .MakeGenericType(domainType, hierarchicalInfo.DirectedLinkType, hierarchicalInfo.UndirectedLinkType, identityInfo.IdentityType);
                
                return (IHierarchicalObjectExpander)ActivatorUtilities.CreateInstance(serviceProvider, expanderType, hierarchicalInfo, identityInfo);

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

    public IHierarchicalObjectExpander<TIdent> Create<TIdent>(Type domainType)
        where TIdent : notnull
    {
        return (IHierarchicalObjectExpander<TIdent>)this.Create(domainType);
    }
}