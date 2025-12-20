using System.Linq.Expressions;

using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IManagedPrincipalConverter<TPrincipal>
{
	Expression<Func<TPrincipal, ManagedPrincipalHeader>> GetToHeaderExpression();

	Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken);
}