using CommonFramework;


namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityRoleInitializer<TSecurityRole> : ISecurityInitializer
{
    Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(IEnumerable<FullSecurityRole> securityRoles, CancellationToken cancellationToken = default);

    new Task<MergeResult<TSecurityRole, FullSecurityRole>> Init(CancellationToken cancellationToken = default);
}
