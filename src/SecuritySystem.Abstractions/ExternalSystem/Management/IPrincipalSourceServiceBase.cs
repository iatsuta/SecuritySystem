using SecuritySystem.Credential;
using SecuritySystem.UserSource;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalSourceServiceBase
{
    Task<IEnumerable<ManagedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken);

    Task<ManagedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken);

    async Task<ManagedPrincipal> GetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken) =>

        await this.TryGetPrincipalAsync(userCredential, cancellationToken)

        ?? throw new UserSourceException($"Principal with id {userCredential} not found");

    Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken);
}