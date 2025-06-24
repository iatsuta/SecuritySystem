using System.Linq.Expressions;
using CommonFramework;


namespace SecuritySystem.Builders.MaterializedBuilder;

public class ManyContextFilterBuilder<TDomainObject, TSecurityContext>(
    SecurityPath<TDomainObject>.ManySecurityPath<TSecurityContext> securityPath,
    SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : ByIdentsFilterBuilder<TDomainObject, TSecurityContext>(securityContextRestriction)
    where TSecurityContext : class, ISecurityContext
{
    protected override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IEnumerable<Guid> securityIdents)
    {
        if (securityPath.Required)
        {
            if (securityPath.SecurityPathQ != null)
            {
                return from securityObjects in securityPath.SecurityPathQ

                       select securityObjects.Any(item => securityIdents.Contains(item.Id));
            }
            else
            {
                return from securityObjects in securityPath.Expression

                       select securityObjects.Any(item => securityIdents.Contains(item.Id));
            }
        }
        else
        {
            if (securityPath.SecurityPathQ != null)
            {
                return from securityObjects in securityPath.SecurityPathQ

                       select !securityObjects.Any()
                              || securityObjects.Any(item => securityIdents.Contains(item.Id));
            }
            else
            {
                return from securityObjects in securityPath.Expression

                       select !securityObjects.Any()
                              || securityObjects.Any(item => securityIdents.Contains(item.Id));
            }
        }
    }
}
