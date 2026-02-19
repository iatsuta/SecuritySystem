using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalManagementService
{
    Type PrincipalType { get; }

    Task<PrincipalData> CreatePrincipalAsync(string principalName, IEnumerable<ManagedPermission> managedPermissions, CancellationToken cancellationToken = default);

    Task<PrincipalData> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken);

    Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default);

    Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(UserCredential userCredential, IEnumerable<ManagedPermission> managedPermissions,
        CancellationToken cancellationToken = default);
}