using System.Collections;
using System.Linq.Expressions;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class ConditionFilterBuilder<TDomainObject>(
    SecurityPath<TDomainObject>.ConditionPath securityPath)
    : SecurityFilterBuilder<TDomainObject>
{
    public override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IReadOnlyDictionary<Type, IEnumerable> _) =>
        securityPath.FilterExpression;
}
