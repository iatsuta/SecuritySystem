using System.Linq.Expressions;

namespace SecuritySystem.Services;

public class SecurityIdentityConverter<TIdent>(IFormatProviderSource formatProviderSource) : ISecurityIdentityConverter<TIdent>
    where TIdent : IParsable<TIdent>
{
    private readonly Expression<Func<TIdent, TIdent>> identityExpr = v => v;

    public SecurityIdentity<TIdent>? TryConvert(SecurityIdentity securityIdentity)
    {
	    return securityIdentity switch
	    {
		    SecurityIdentity<TIdent> typedSecurityIdentity => typedSecurityIdentity,
		    SecurityIdentity<string> { Id: var stringId } when TIdent.TryParse(stringId, formatProviderSource.FormatProvider, out var id) =>
			    new SecurityIdentity<TIdent>(id),
		    _ => null
	    };
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