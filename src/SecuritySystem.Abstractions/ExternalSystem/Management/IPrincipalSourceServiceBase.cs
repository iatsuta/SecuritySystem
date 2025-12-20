using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalSourceServiceBase
{
    Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken);

    Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken);

    Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken);
}