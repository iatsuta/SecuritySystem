using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface ISecurityIdentityConverter
{
    TypedSecurityIdentity? TryConvert(SecurityIdentity securityIdentity);

    TypedSecurityIdentity Convert(SecurityIdentity securityIdentity);
}

public interface ISecurityIdentityConverter<TIdent> : ISecurityIdentityConverter
    where TIdent : notnull
{
    new TypedSecurityIdentity<TIdent>? TryConvert(SecurityIdentity securityIdentity);

    new TypedSecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity);

	Expression<Func<TSourceIdent, TIdent>> GetConvertExpression<TSourceIdent>();
}