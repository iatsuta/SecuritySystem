using System.Linq.Expressions;

using CommonFramework;

namespace SecuritySystem.Builders.MaterializedBuilder;

public class ManyContextFilterBuilder<TDomainObject, TSecurityContext, TIdent>(
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction,
    IdentityInfo<TSecurityContext, TIdent> identityInfo)
    : ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TIdent>(securityContextRestriction)
    where TSecurityContext : class, ISecurityContext
    where TIdent : notnull
{
    protected override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(TIdent[] permissionIdents)
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