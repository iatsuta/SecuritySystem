﻿using System.Linq.Expressions;

using CommonFramework;

using SecuritySystem.ExpressionEvaluate;
using SecuritySystem.ExternalSystem;
using SecuritySystem.HierarchicalExpand;

namespace SecuritySystem.Builders.QueryBuilder;

public class ManyContextFilterBuilder<TPermission, TDomainObject, TSecurityContext>(
    IPermissionSystem<TPermission> permissionSystem,
    IHierarchicalObjectExpanderFactory<Guid> hierarchicalObjectExpanderFactory,
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : SecurityFilterBuilder<TPermission, TDomainObject>
    where TSecurityContext : class, ISecurityContext
{
    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(
        HierarchicalExpandType expandType)
    {
        var allowGrandAccess = securityContextRestriction?.Required != true;

        var grandAccessExpr = allowGrandAccess
            ? permissionSystem.GetGrandAccessExpr<TSecurityContext>()
            : _ => false;

        var getIdents = permissionSystem.GetPermissionRestrictionsExpr(securityContextRestriction?.Filter);

        var expander = hierarchicalObjectExpanderFactory.CreateQuery(typeof(TSecurityContext));

        var expandExpression = expander.GetExpandExpression(expandType);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>
        {
            var expandExpressionQ =

                from idents in getIdents

                select ee.Evaluate(expandExpression, idents);

            if (securityPath.Required)
            {
                if (securityPath.SecurityPathQ != null)
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || ee.Evaluate(securityPath.SecurityPathQ, domainObject)
                                                             .Any(item => ee.Evaluate(expandExpressionQ, permission).Contains(item.Id));
                }
                else
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || ee.Evaluate(securityPath.Expression, domainObject)
                                                             .Any(item => ee.Evaluate(expandExpressionQ, permission).Contains(item.Id));
                }
            }
            else
            {
                if (securityPath.SecurityPathQ != null)
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || !ee.Evaluate(securityPath.SecurityPathQ, domainObject).Any()

                                                         || ee.Evaluate(securityPath.SecurityPathQ, domainObject).Any(item =>
                                                             ee.Evaluate(getIdents, permission).Contains(item.Id));
                }
                else
                {
                    return (domainObject, permission) => ee.Evaluate(grandAccessExpr, permission)

                                                         || !ee.Evaluate(securityPath.Expression, domainObject).Any()

                                                         || ee.Evaluate(securityPath.Expression, domainObject).Any(item =>
                                                             ee.Evaluate(getIdents, permission).Contains(item.Id));
                }
            }
        });
    }
}