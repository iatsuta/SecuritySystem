using System.Linq.Expressions;

namespace SecuritySystem.Services;

public class SecurityIdentityConverter<TIdent> : ISecurityIdentityConverter<TIdent>
    where TIdent : notnull
{
    private readonly Expression<Func<TIdent, TIdent>> identityExpr = v => v;

    public SecurityIdentity<TIdent>? TryConvert(SecurityIdentity securityIdentity)
    {
        if (securityIdentity is SecurityIdentity<TIdent> typedSecurityIdentity)
        {
            return typedSecurityIdentity;
        }
        else
        {
            return null;
        }
    }

    public SecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity)
    {
        return this.TryConvert(securityIdentity) ?? throw new ArgumentOutOfRangeException(nameof(securityIdentity));
    }

    public Expression<Func<TSourceIdent, TIdent>> GetConvertExpression<TSourceIdent>()
    {
        if (identityExpr is Expression<Func<TSourceIdent, TIdent>> result)
        {
            return result;
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}