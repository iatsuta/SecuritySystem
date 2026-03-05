using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.Services;

public interface IManagedPrincipalConverter<in TPrincipal>
{
	ValueTask<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal dbPrincipal, CancellationToken cancellationToken);
}