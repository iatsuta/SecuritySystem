using System.Linq.Expressions;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class ConditionFilterBuilder<TPermission, TDomainObject>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    SecurityPath<TDomainObject>.ConditionPath securityPath)
    : AccessorsFilterBuilder<TPermission, TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator = expressionEvaluatorStorage.GetForType(typeof(ConditionFilterBuilder<TPermission, TDomainObject>));

    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType)
    {
        var hasAccess = this.expressionEvaluator.Evaluate(securityPath.FilterExpression, domainObject);

        return _ => hasAccess;
    }
}