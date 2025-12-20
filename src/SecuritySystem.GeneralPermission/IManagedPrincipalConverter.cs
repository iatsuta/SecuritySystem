using SecuritySystem.ExternalSystem.Management;

namespace SecuritySystem.GeneralPermission;

public interface IManagedPrincipalConverter<in TPrincipal>
{
	Task<ManagedPrincipal> ToManagedPrincipalAsync(TPrincipal principal, CancellationToken cancellationToken);
}