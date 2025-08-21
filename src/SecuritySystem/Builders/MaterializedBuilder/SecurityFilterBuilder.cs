using System.Linq.Expressions;

namespace SecuritySystem.Builders.MaterializedBuilder;

public abstract class SecurityFilterBuilder<TDomainObject>
{
    public abstract Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(Dictionary<Type, Array> permission);
}
