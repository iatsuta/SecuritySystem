using SecuritySystem.HierarchicalExpand;

using System.Linq.Expressions;

namespace SecuritySystem.Builders.AccessorsBuilder;

public abstract class AccessorsFilterBuilder<TPermission, TDomainObject>
{
    public abstract Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType);
}
