using CommonFramework;

namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityRoleInitializer<TSecurityRole> : ISecurityRoleInitializer
{
    Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(IEnumerable<FullSecurityRole> securityRoles, CancellationToken cancellationToken);

    new Task<MergeResult<TSecurityRole, FullSecurityRole>> Initialize(CancellationToken cancellationToken);
}

public interface ISecurityRoleInitializer : IInitializer;