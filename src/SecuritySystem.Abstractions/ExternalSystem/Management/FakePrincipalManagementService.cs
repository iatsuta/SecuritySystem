using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public class FakePrincipalManagementService : IPrincipalManagementService
{
    public Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IIdentityObject<Guid>> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IIdentityObject<Guid>> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IIdentityObject<Guid>> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MergeResult<IIdentityObject<Guid>, IIdentityObject<Guid>>> UpdatePermissionsAsync(Guid principalId, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}