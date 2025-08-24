using CommonFramework;

using SecuritySystem.Credential;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalManagementService : IPrincipalSourceService
{
    Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default);

    Task<object> UpdatePrincipalNameAsync(UserCredential userCredential, string principalName, CancellationToken cancellationToken);

    Task<object> RemovePrincipalAsync(UserCredential userCredential, bool force, CancellationToken cancellationToken = default);

    Task<MergeResult<object, object>> UpdatePermissionsAsync(Guid principalId, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken = default);
}