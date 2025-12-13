using System.Linq.Expressions;

using HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public abstract class SecurityFilterBuilder<TPermission, TDomainObject>
{
    public abstract Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType);
}
