using System.Linq.Expressions;

using CommonFramework.ExpressionEvaluate;

using HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public class NestedManyFilterBuilder<TDomainObject, TPermission, TNestedObject>(
    SecurityFilterBuilderFactory<TNestedObject, TPermission> nestedBuilderFactory,
    SecurityPath<TDomainObject>.NestedManySecurityPath<TNestedObject> securityPath,
    IReadOnlyList<SecurityContextRestriction> securityContextRestrictions) : SecurityFilterBuilder<TDomainObject, TPermission>
{
    private SecurityFilterBuilder<TNestedObject, TPermission> NestedBuilder { get; } =
        nestedBuilderFactory.CreateBuilder(securityPath.NestedSecurityPath, securityContextRestrictions);

    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType)
    {
        var baseFilter = this.NestedBuilder.GetSecurityFilterExpression(expandType);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>
        {
            if (securityPath.Required)
            {
                return (domainObject, permission) => ee.Evaluate(securityPath.NestedExpression, domainObject)
                    .Any(nestedObject => ee.Evaluate(baseFilter, nestedObject, permission));
            }
            else
            {
                return (domainObject, permission) => !ee.Evaluate(securityPath.NestedExpression, domainObject).Any()

                                                     || ee.Evaluate(securityPath.NestedExpression, domainObject)
                                                         .Any(nestedObject => ee.Evaluate(baseFilter, nestedObject, permission));
            }
        });
    }
}