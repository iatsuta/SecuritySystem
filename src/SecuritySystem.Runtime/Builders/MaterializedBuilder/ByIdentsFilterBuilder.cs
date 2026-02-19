using System.Collections;
using System.Linq.Expressions;

namespace SecuritySystem.Builders.MaterializedBuilder;

public abstract class ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TSecurityContextIdent>(SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : SecurityFilterBuilder<TDomainObject>
    where TSecurityContext : class, ISecurityContext
{
    public sealed override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IReadOnlyDictionary<Type, IEnumerable> permission)
    {
        if (permission.TryGetValue(typeof(TSecurityContext), out var securityIdents))
        {
            return this.GetSecurityFilterExpression((IEnumerable<TSecurityContextIdent>)securityIdents);
        }
        else
        {
            var allowsUnrestrictedAccess = securityContextRestriction?.Required != true;

            return _ => allowsUnrestrictedAccess;
        }
    }

    protected abstract Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(IEnumerable<TSecurityContextIdent> permissionIdents);
}