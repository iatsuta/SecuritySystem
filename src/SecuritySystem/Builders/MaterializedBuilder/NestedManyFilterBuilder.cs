using System.Linq.Expressions;
using CommonFramework;
using SecuritySystem.ExpressionEvaluate;


namespace SecuritySystem.Builders.MaterializedBuilder;

public class NestedManyFilterBuilder<TDomainObject, TNestedObject>(
    SecurityFilterBuilderFactory<TNestedObject> nestedBuilderFactory,
    SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions) : SecurityFilterBuilder<TDomainObject>
{
    private SecurityFilterBuilder<TNestedObject> NestedBuilder { get; } = nestedBuilderFactory.CreateBuilder(securityPath.NestedSecurityPath, securityContextRestrictions);

    public override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(Dictionary<Type, Array> permission)
    {
        var nestedFilterExpression = this.NestedBuilder.GetSecurityFilterExpression(permission);

        var nestedCollectionFilterExpression = nestedFilterExpression.ToCollectionFilter();

        var mainCondition = ExpressionEvaluateHelper.InlineEvaluate(ee => securityPath.NestedExpression.Select(v => ee.Evaluate(nestedCollectionFilterExpression, v).Any()));

        if (securityPath.Required)
        {
            return mainCondition;
        }
        else
        {
            var emptyCondition = securityPath.NestedExpression.Select(v => !v.Any());

            return emptyCondition.BuildOr(mainCondition);
        }
    }
}