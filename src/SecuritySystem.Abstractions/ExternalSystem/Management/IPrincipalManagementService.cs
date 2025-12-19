using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalManagementService
{
    Type PrincipalType { get; }

    Task<PrincipalData> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default);

    Task<PrincipalData> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken);

    Task<PrincipalData> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default);

    Task<MergeResult<PermissionData, PermissionData>> UpdatePermissionsAsync(UserCredential userCredential, IEnumerable<TypedPermission> typedPermissions,
        CancellationToken cancellationToken = default);
}