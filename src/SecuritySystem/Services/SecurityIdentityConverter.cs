using System.Linq.Expressions;

namespace SecuritySystem.Services;

public class SecurityIdentityConverter<TIdent>(IFormatProviderSource formatProviderSource) : ISecurityIdentityConverter<TIdent>
    where TIdent : IParsable<TIdent>, new()
{
    private readonly Expression<Func<TIdent, TIdent>> identityExpr = v => v;

    public TypedSecurityIdentity<TIdent>? TryConvert(SecurityIdentity securityIdentity)
    {
        return securityIdentity switch
        {
            TypedSecurityIdentity<TIdent> typedSecurityIdentity => typedSecurityIdentity,

            UntypedSecurityIdentity i when i == SecurityIdentity.Default => TypedSecurityIdentity.Create(new TIdent()),

            UntypedSecurityIdentity { Id: var rawId } when TIdent.TryParse(rawId, formatProviderSource.FormatProvider, out var id) =>
                TypedSecurityIdentity.Create(id),

            TypedSecurityIdentity<string> { Id: var stringId } when TIdent.TryParse(stringId, formatProviderSource.FormatProvider, out var id) =>
                TypedSecurityIdentity.Create(id),

            _ => null
        };
    }

    TypedSecurityIdentity ISecurityIdentityConverter.Convert(SecurityIdentity securityIdentity)
    {
        return this.Convert(securityIdentity);
    }

    public TypedSecurityIdentity<TIdent> Convert(SecurityIdentity securityIdentity)
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

    TypedSecurityIdentity? ISecurityIdentityConverter.TryConvert(SecurityIdentity securityIdentity) => this.TryConvert(securityIdentity);
}