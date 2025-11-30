using CommonFramework;


namespace SecuritySystem.GeneralPermission.Initialize;

public interface ITemplateSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
