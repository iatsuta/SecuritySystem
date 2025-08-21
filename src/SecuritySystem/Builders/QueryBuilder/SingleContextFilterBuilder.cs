using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public class SingleContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TIdent>(
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : SecurityFilterBuilder<TPermission, TDomainObject>
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(
        HierarchicalExpandType expandType)
    {
        var allowGrandAccess = securityContextRestriction?.Required != true;

        var grandAccessExpr = allowGrandAccess
            ? permissionSystem.GetGrandAccessExpr<TSecurityContext>()
            : _ => false;

        var getIdents = permissionSystem.GetPermissionRestrictionsExpr<TSecurityContext, TIdent>(securityContextRestriction?.Filter);

        var expander = hierarchicalObjectExpanderFactory.CreateQuery<TIdent>(typeof(TSecurityContext));

        var expandExpression = expander.GetExpandExpression(expandType);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>
        {
            var expandExpressionQ =

                from idents in getIdents

                select ee.Evaluate(expandExpression, idents);

            if (securityPath.Required)
            {
                return (domainObject, permission) =>

                    ee.Evaluate(grandAccessExpr, permission)

                    || ee.Evaluate(expandExpressionQ, permission).Contains(
                        ee.Evaluate(securityPath.Expression, domainObject)!.Id);
            }
            else
            {
                return (domainObject, permission) =>

                    ee.Evaluate(grandAccessExpr, permission)

                    || ee.Evaluate(securityPath.Expression, domainObject) == null

                    || ee.Evaluate(expandExpressionQ, permission).Contains(
                        ee.Evaluate(securityPath.Expression, domainObject)!.Id);
            }
        });
    }
}