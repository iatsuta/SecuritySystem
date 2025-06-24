using SecuritySystem.HierarchicalExpand;
using System.Linq.Expressions;

namespace SecuritySystem.Builders.QueryBuilder;

public abstract class SecurityFilterBuilder<TPermission, TDomainObject>
{
    public abstract Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType);
}
