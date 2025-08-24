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

    public Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<object> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<object> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<MergeResult<object, object>> UpdatePermissionsAsync(Guid principalId, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}