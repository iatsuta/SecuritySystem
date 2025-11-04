using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Services;

public interface IHierarchicalInfoSource
{
    HierarchicalInfo<TDomainObject> GetHierarchicalInfo<TDomainObject>();

    bool IsHierarchical(Type domainType);
}