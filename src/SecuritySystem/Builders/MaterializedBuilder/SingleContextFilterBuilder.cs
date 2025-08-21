using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class SingleContextFilterBuilder<TDomainObject, TSecurityContext, TIdent>(
    SecurityPath<TDomainObject>.SingleSecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TIdent> identityInfo)
    : ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TIdent>(securityContextRestriction)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(TIdent[] permissionIdents)
    {
        var singleFilter = identityInfo.CreateContainsFilter(permissionIdents);

        var containsFilterExpr = securityPath.Expression!.Select(singleFilter);

        if (securityPath.Required)
        {
            return containsFilterExpr;
        }
        else
        {
            var grandAccessFilter = securityPath.Expression.Select(securityObject => securityObject == null);

            return grandAccessFilter.BuildOr(containsFilterExpr);
        }
    }
}