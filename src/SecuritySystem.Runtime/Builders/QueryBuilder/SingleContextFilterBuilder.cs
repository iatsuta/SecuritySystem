using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.ExpressionEvaluate;
using CommonFramework.IdentitySource;
using HierarchicalExpand;
using SecuritySystem.ExternalSystem;

namespace SecuritySystem.Builders.QueryBuilder;

public class SingleContextFilterBuilder<TDomainObject, TPermission, TSecurityContext, TSecurityContextIdent>(
	IPermissionSystem<TPermission> permissionSystem,
	IHierarchicalObjectExpanderFactory hierarchicalObjectExpanderFactory,
	SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
	SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
	IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
	: SecurityFilterBuilder<TDomainObject, TPermission>
	where TSecurityContext : class, ISecurityContext
	where TSecurityContextIdent : notnull
{
    private readonly IPermissionRestrictionSource<TPermission, TSecurityContextIdent> permissionRestrictionSource =
        permissionSystem.GetRestrictionSource<TSecurityContext, TSecurityContextIdent>(securityContextRestriction?.Filter);

    public override Expression<Func<TDomainObject, TPermission, bool>> GetSecurityFilterExpression(HierarchicalExpandType expandType)
	{
		var allowGrandAccess = securityContextRestriction?.Required != true;

		var grandAccessExpr = allowGrandAccess
			? permissionRestrictionSource.GetGrandAccessExpr()
			: _ => false;

		var getIdents = permissionRestrictionSource.GetIdentsExpr();

		var expander = hierarchicalObjectExpanderFactory.Create<TSecurityContextIdent>(typeof(TSecurityContext));

		var expandExpression = expander.GetExpandExpression(expandType);

		var fullIdPath = securityPath.Expression!.Select(identityInfo.Id.Path);

        var expandExpressionQ = getIdents.Select(expandExpression);

        return ExpressionEvaluateHelper.InlineEvaluate<Func<TDomainObject, TPermission, bool>>(ee =>
		{
			if (securityPath.Required)
			{
				return (domainObject, permission) =>

					ee.Evaluate(grandAccessExpr, permission)

					|| ee.Evaluate(expandExpressionQ, permission).Contains(
						ee.Evaluate(fullIdPath, domainObject));
			}
			else
			{
				return (domainObject, permission) =>

					ee.Evaluate(grandAccessExpr, permission)

					|| ee.Evaluate(securityPath.Expression, domainObject) == null

					|| ee.Evaluate(expandExpressionQ, permission).Contains(
						ee.Evaluate(fullIdPath, domainObject));
			}
		});
	}
}