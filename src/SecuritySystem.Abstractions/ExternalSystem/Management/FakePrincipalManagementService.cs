using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public class FakePrincipalManagementService : IPrincipalManagementService
{
    public Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TypedPrincipal?> TryGetPrincipalAsync(UserCredential userCredential, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PrincipalData> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PrincipalData> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(UserCredential userCredential, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}