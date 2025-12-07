using CommonFramework;

namespace SecuritySystem.GeneralPermission.Initialize.SecurityContext;

public interface ISecurityContextInitializer<TSecurityContextType> : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken);
}
