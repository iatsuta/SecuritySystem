using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;

namespace SecuritySystem.Providers;

public class ConditionSecurityProvider<TDomainObject>(Expression<Func<TDomainObject, bool>> securityFilter, IExpressionEvaluatorStorage expressionEvaluatorStorage)
    : ISecurityProvider<TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(ConditionSecurityProvider<TDomainObject>));

    public IQueryable<TDomainObject> InjectFilter(IQueryable<TDomainObject> queryable)
    {
        return queryable.Where(securityFilter);
    }

    public bool HasAccess(TDomainObject domainObject)
    {
        return this.expressionEvaluator.Evaluate(securityFilter, domainObject);
    }
}