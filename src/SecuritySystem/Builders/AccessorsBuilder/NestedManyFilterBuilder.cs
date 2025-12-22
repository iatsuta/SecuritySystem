using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class NestedManyFilterBuilder<TPermission, TDomainObject, TNestedObject>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    AccessorsFilterBuilderFactory<TPermission, TNestedObject> nestedBuilderFactory,
    SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions) : AccessorsFilterBuilder<TPermission, TDomainObject>
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(NestedManyFilterBuilder<TPermission, TDomainObject, TNestedObject>));

    private AccessorsFilterBuilder<TPermission, TNestedObject> NestedBuilder { get; } =
        nestedBuilderFactory.CreateBuilder(securityPath.NestedSecurityPath, securityContextRestrictions);

    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType) =>

        this.expressionEvaluator.Evaluate(securityPath.NestedExpression, domainObject).BuildOr(item => this.NestedBuilder.GetAccessorsFilter(item, expandType));
}