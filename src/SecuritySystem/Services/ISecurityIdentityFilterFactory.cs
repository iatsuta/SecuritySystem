using System.Linq.Expressions;

namespace SecuritySystem.Services;

public interface ISecurityIdentityFilterFactory<TDomainObject>
{
	Expression<Func<TDomainObject, bool>> CreateFilter(TypedSecurityIdentity securityIdentity);
}