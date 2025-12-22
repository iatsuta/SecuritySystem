using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface IManagedPrincipalConverter<in TPrincipal>
{
	Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken);
}