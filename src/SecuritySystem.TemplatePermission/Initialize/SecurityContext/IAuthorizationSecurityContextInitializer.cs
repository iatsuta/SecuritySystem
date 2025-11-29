using CommonFramework;


namespace SecuritySystem.TemplatePermission.Initialize;

public interface IAuthorizationSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
