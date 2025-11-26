using CommonFramework;


namespace SecuritySystem.TemplatePermission.Initialize;

public interface IAuthorizationSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<SecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
