using System.Linq.Expressions;

using HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public abstract class AccessorsFilterBuilder<TDomainObject, TPermission>
{
    public abstract Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType);
}
