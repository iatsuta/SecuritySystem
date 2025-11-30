using CommonFramework;

namespace SecuritySystem.ExternalSystem.Management;

public class FakePrincipalManagementService : IPrincipalManagementService
{
    public Task<IEnumerable<TypedPrincipalHeader>> GetPrincipalsAsync(string nameFilter, int limit, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<TypedPrincipal?> TryGetPrincipalAsync(string principalId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetLinkedPrincipalsAsync(IEnumerable<SecurityRole> securityRoles, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<object> UpdatePrincipalNameAsync(string principalId, string principalName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<object> RemovePrincipalAsync(string principalId, bool force, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<MergeResult<object, object>> UpdatePermissionsAsync(string principalId, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}