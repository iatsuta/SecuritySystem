using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using HierarchicalExpand;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.QueryBuilder;

public class ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext, TSecurityContextIdent>(
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
    : SecurityFilterBuilder<TPermission, TDomainObject>
    where TSecurityContext : class, ISecurityContext
    where TSecurityContextIdent : notnull
{
    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType)
    {
        var allowGrandAccess = securityContextRestriction?.Required != true;

        var grandAccessExpr = allowGrandAccess
            ? permissionSystem.GetGrandAccessExpr<TSecurityContext, TSecurityContextIdent>()
            : _ => false;

        var getIdents = permissionSystem.GetPermissionRestrictionsExpr<TSecurityContext, TSecurityContextIdent>(securityContextRestriction?.Filter);

        var expander = hierarchicalObjectExpanderFactory.Create<TSecurityContextIdent>(typeof(TSecurityContext));

        var expandExpression = expander.GetExpandExpression(expandType);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>
        {
            var expandExpressionQ =

                from idents in getIdents

                select ee.Evaluate(expandExpression, idents);


            if (securityPath.SecurityPathQ != null)
            {
                if (securityPath.Required)
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || ee.Evaluate(securityPath.SecurityPathQ, domainObject)
                                                             .Any(item => ee.Evaluate(expandExpressionQ, permission).Contains(ee.Evaluate(identityInfo.Id.Path, item)));
                }
                else
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                                  || !ee.Evaluate(securityPath.SecurityPathQ, domainObject).Any()

                                                                  || ee.Evaluate(securityPath.SecurityPathQ, domainObject).Any(item =>
                                                                      ee.Evaluate(getIdents, permission).Contains(ee.Evaluate(identityInfo.Id.Path, item)));
                }
            }
            else
            {
                if (securityPath.Required)
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || ee.Evaluate(securityPath.Expression, domainObject)
                                                             .Any(item => ee.Evaluate(expandExpressionQ, permission).Contains(ee.Evaluate(identityInfo.Id.Path, item)));
                }
                else
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || !ee.Evaluate(securityPath.Expression, domainObject).Any()

                                                         || ee.Evaluate(securityPath.Expression, domainObject).Any(item =>
                                                             ee.Evaluate(getIdents, permission).Contains(ee.Evaluate(identityInfo.Id.Path, item)));
                }
            }
        });
    }
}