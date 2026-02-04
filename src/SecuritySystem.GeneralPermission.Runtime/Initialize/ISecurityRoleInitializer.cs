using CommonFramework;

namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityRoleInitializer<TSecurityRole> : ISecurityRoleInitializer
{
    Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(IEnumerable<FullSecurityRole> securityRoles, CancellationToken cancellationToken);

    new Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(CancellationToken cancellationToken);
}

public interface ISecurityRoleInitializer : ISecurityInitializer;