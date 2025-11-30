using CommonFramework;

namespace SecuritySystem.ExternalSystem.Management;

public interface IPrincipalManagementService : IPrincipalSourceService
{
    Task<object> CreatePrincipalAsync(string principalName, CancellationToken cancellationToken = default);

    Task<object> UpdatePrincipalNameAsync(string principalId, string principalName, CancellationToken cancellationToken);

    Task<object> RemovePrincipalAsync(string principalId, bool force, CancellationToken cancellationToken = default);

    Task<MergeResult<object, object>> UpdatePermissionsAsync(string principalId, IEnumerable<TypedPermission> typedPermissions, CancellationToken cancellationToken = default);
}