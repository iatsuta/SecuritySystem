using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public class FakePrincipalManagementService : IPrincipalManagementService
{
    public Type PrincipalType => throw new InvalidOperationException();

    public Task<PrincipalData> CreatePrincipalAsync(string principalName, IEnumerable<ManagedPermission> managedPermissions, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException();
    }

    public Task<PrincipalData> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

    public Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }

    public Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(UserCredential userCredential, IEnumerable<ManagedPermission> managedPermissions, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException();
    }
}