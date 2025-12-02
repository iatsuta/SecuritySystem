using System.Linq.Expressions;
using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface ITypedPrincipalConverter<TPrincipal>
{
	Expression<Func<TPrincipal, TypedPrincipalHeader>> GetToHeaderExpression();

	Task<TypedPrincipal> ToTypedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken);
}