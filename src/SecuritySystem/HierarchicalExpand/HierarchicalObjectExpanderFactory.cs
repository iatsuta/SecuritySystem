using CommonFramework;

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace SecuritySystem.HierarchicalExpand;

public class HierarchicalObjectExpanderFactory<TIdent>(IServiceProvider serviceProvider, IRealTypeResolver? realTypeResolver = null) : IHierarchicalObjectExpanderFactory<TIdent>
    where TIdent : struct
{
    private static readonly MethodInfo GenericCreateMethod =
        typeof(HierarchicalObjectExpanderFactory<TIdent>).GetMethod(nameof(Create), BindingFlags.Public | BindingFlags.Instance)!;

    public virtual IHierarchicalObjectExpander<TIdent> Create<TDomainObject>()
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

                return (IHierarchicalObjectExpander<TIdent>)ActivatorUtilities.CreateInstance(serviceProvider, expanderType, hierarchicalInfo);

            }
            else
            {
                return this.CreatePlain<TDomainObject>();
            }
        }
    }

    protected virtual IHierarchicalObjectExpander<TIdent> CreatePlain<TDomainObject>()
    {
        return new PlainHierarchicalObjectExpander<TIdent>();
    }

    IHierarchicalObjectExpander<TIdent> IHierarchicalObjectExpanderFactory<TIdent>.Create(Type domainType)
    {
        return GenericCreateMethod.MakeGenericMethod(domainType).Invoke<IHierarchicalObjectExpander<TIdent>>(this, []);
    }

    IHierarchicalObjectQueryableExpander<TIdent> IHierarchicalObjectExpanderFactory<TIdent>.CreateQuery(Type domainType)
    {
        return GenericCreateMethod.MakeGenericMethod(domainType).Invoke<IHierarchicalObjectQueryableExpander<TIdent>>(this, []);
    }
}