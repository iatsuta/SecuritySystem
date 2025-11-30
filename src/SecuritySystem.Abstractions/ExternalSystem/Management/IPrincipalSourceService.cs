namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalSourceService
{
    Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken);

    Task<TypedPrincipal?> TryGetPrincipalAsync(string principalId, CancellationToken cancellationToken);

    Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken);
}
