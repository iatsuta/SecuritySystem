using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;

using HierarchicalExpand;

namespace SecuritySystem.Builders.AccessorsBuilder;

public class NestedManyFilterBuilder<TDomainObject, TPermission, TNestedObject>(
    IExpressionEvaluatorStorage expressionEvaluatorStorage,
    AccessorsFilterBuilderFactory<TNestedObject, TPermission> nestedBuilderFactory,
    SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions) : AccessorsFilterBuilder<TDomainObject, TPermission>
{
    private readonly IExpressionEvaluator expressionEvaluator =
        expressionEvaluatorStorage.GetForType(typeof(NestedManyFilterBuilder<TDomainObject, TPermission, TNestedObject>));

    private AccessorsFilterBuilder<TNestedObject, TPermission> NestedBuilder { get; } =
        nestedBuilderFactory.CreateBuilder(securityPath.NestedSecurityPath, securityContextRestrictions);

    public override Expression<Func<TPermission, bool>> GetAccessorsFilter(TDomainObject domainObject, HierarchicalExpandType expandType) =>

        this.expressionEvaluator.Evaluate(securityPath.NestedExpression, domainObject).BuildOr(item => this.NestedBuilder.GetAccessorsFilter(item, expandType));
}