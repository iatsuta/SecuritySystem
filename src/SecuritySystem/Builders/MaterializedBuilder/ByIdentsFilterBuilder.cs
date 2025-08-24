using System.Linq.Expressions;

namespace SecuritySystem.Builders.MaterializedBuilder;

public abstract class ByIdentsFilterBuilder<TDomainObject, TSecurityContext, TIdent>(SecurityContextRestriction<TSecurityContext>? securityContextRestriction)
    : SecurityFilterBuilder<TDomainObject>
    where TSecurityContext : class, ISecurityContext
{
    public sealed override Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(Dictionary<Type, Array> permission)
    {
        if (permission.TryGetValue(typeof(TSecurityContext), out var securityIdents))
        {
            return this.GetSecurityFilterExpression((TIdent[])securityIdents);
        }
        else
        {
            var allowGrandAccess = securityContextRestriction?.Required != true;

            return _ => allowGrandAccess;
        }
    }

    protected abstract Expression<Func<TDomainObject, bool>> GetSecurityFilterExpression(TIdent[] permissionIdents);
}