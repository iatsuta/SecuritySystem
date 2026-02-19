using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface IManagedPrincipalConverter<in TPrincipal>
{
	Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal dbPrincipal, CancellationToken cancellationToken);
}