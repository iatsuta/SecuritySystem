using CommonFramework;


namespace SecuritySystem.TemplatePermission.Initialize;

public interface ITemplateSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
