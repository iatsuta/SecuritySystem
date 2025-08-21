using CommonFramework;

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using SecuritySystem.Services;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectExpanderFactory(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource, IRealTypeResolver? realTypeResolver = null)
    : IHierarchicalObjectExpanderFactory
{
    private static readonly MethodInfo GenericCreateMethod =
        typeof(HierarchicalObjectExpanderFactory).GetMethod(nameof(Create), BindingFlags.Public | BindingFlags.Instance)!;

    public virtual IHierarchicalObjectExpander<TIdent> Create<TDomainObject, TIdent>()
        where TIdent : struct
    {
        var realType = realTypeResolver?.Resolve(typeof(TDomainObject)) ?? typeof(TDomainObject);

        if (realType != typeof(TDomainObject))
        {
            return GenericCreateMethod.MakeGenericMethod(realType).Invoke<IHierarchicalObjectExpander<TIdent>>(this, []);
        }
        else
        {
            var hierarchicalInfo = serviceProvider.GetService<HierarchicalInfo<TDomainObject>>();

            if (hierarchicalInfo != null)
            {
                var expanderType = typeof(HierarchicalObjectAncestorLinkExpander<,,,>)
                    .MakeGenericType(typeof(TDomainObject), hierarchicalInfo.DirectedLinkType, hierarchicalInfo.UndirectedLinkType, typeof(TIdent));

                var identityInfo = identityInfoSource.GetIdentityInfo(typeof(TDomainObject));

                return (IHierarchicalObjectExpander<TIdent>)ActivatorUtilities.CreateInstance(serviceProvider, expanderType, hierarchicalInfo, identityInfo);

            }
            else
            {
                return this.CreatePlain<TDomainObject, TIdent>();
            }
        }
    }

    protected virtual IHierarchicalObjectExpander<TIdent> CreatePlain<TDomainObject, TIdent>()
        where TIdent : notnull
    {
        return new PlainHierarchicalObjectExpander<TIdent>();
    }

    IHierarchicalObjectExpander<TIdent> IHierarchicalObjectExpanderFactory.Create<TIdent>(Type domainType)
    {
        return GenericCreateMethod.MakeGenericMethod(domainType).Invoke<IHierarchicalObjectExpander<TIdent>>(this, []);
    }

    IHierarchicalObjectQueryableExpander<TIdent> IHierarchicalObjectExpanderFactory.CreateQuery<TIdent>(Type domainType)
    {
        return GenericCreateMethod.MakeGenericMethod(domainType).Invoke<IHierarchicalObjectQueryableExpander<TIdent>>(this, []);
    }
}