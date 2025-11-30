using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface ISecurityIdentityConverter<TIdent>
	where TIdent : notnull
{
    SecurityIdentity<TIdent>? TryConvert(SecurityIdentity securityIdentity);

    SecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity);

	Expression<Func<TSourceIdent, TIdent>> GetConvertExpression<TSourceIdent>();
}