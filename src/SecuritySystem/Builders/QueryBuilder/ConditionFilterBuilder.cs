using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;
using HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public class ConditionFilterBuilder<TPermission, TDomainObject>(
    SecurityPath<TDomainObject>.ConditionPath securityPath)
    : SecurityFilterBuilder<TPermission, TDomainObject>
{
    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(
        HierarchicalExpandType expandType)
    {
        var securityFilter = securityPath.FilterExpression;

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>

            (domainObject, _) => ee.Evaluate(securityFilter, domainObject));
    }
}