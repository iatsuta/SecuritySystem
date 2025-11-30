using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class ManyContextFilterBuilder<TDomainObject, TSecurityContext, TSecurityContextIdent>(
	SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
	SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
	IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
	: ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TSecurityContextIdent>(securityContextRestriction)
	where TSecurityContext : class, ISecurityContext
	where TSecurityContextIdent : notnull
{
	protected override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IEnumerable<TSecurityContextIdent> permissionIdents)
	{
		var singleFilter = identityInfo.CreateContainsFilter(permissionIdents);

		var containsFilterExpr = securityPath.Expression.Select(singleFilter.ToCollectionFilter()).Select(securityContext => securityContext.Any());

		if (securityPath.Required)
		{
			if (securityPath.SecurityPathQ != null)
			{
				return from securityObjects in securityPath.SecurityPathQ

					select securityObjects.Any(singleFilter);
			}
			else
			{
				return containsFilterExpr;
			}
		}
		else
		{
			if (securityPath.SecurityPathQ != null)
			{
				return from securityObjects in securityPath.SecurityPathQ

					select !securityObjects.Any() || securityObjects.Any(singleFilter);
			}
			else
			{
				var grandAccessFilter = securityPath.Expression.Select(securityObjects => !securityObjects.Any());

				return grandAccessFilter.BuildOr(containsFilterExpr);
			}
		}
	}
}