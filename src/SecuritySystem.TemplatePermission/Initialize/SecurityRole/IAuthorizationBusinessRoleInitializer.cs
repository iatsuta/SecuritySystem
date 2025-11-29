using CommonFramework;


namespace SecuritySystem.TemplatePermission.Initialize;

public interface IAuthorizationBusinessRoleInitializer : ISecurityInitializer
{
    Task<MergeResult<TBusinessRole, FullSecurityRole>> Init(
        IEnumerable<FullSecurityRole> securityRoles,
        CancellationToken cancellationToken = default);

    new Task<MergeResult<TBusinessRole, FullSecurityRole>> Init(CancellationToken cancellationToken = default);
}
