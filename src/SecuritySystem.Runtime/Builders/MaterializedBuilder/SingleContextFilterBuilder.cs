using System.Linq.Expressions;

using CommonFramework;
using CommonFramework.IdentitySource;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class SingleContextFilterBuilder<TDomainObject, TSecurityContext, TSecurityContextIdent>(
	SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
	SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
	IdentityInfo<TSecurityContext, TSecurityContextIdent> identityInfo)
	: ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TSecurityContextIdent>(securityContextRestriction)
	where TSecurityContext : class, ISecurityContext
	where TSecurityContextIdent : notnull
{
	protected override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IEnumerable<TSecurityContextIdent> permissionIdents)
	{
		var singleFilter = identityInfo.CreateContainsFilter(permissionIdents);

		var containsFilterExpr = securityPath.Expression!.Select(singleFilter);

		if (securityPath.Required)
		{
			return containsFilterExpr;
		}
		else
		{
			var unrestrictedFilter = securityPath.Expression.Select(securityObject => securityObject == null);

			return unrestrictedFilter.BuildOr(containsFilterExpr);
		}
	}
}