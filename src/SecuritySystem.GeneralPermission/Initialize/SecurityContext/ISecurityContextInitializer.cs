using CommonFramework;


namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityContextInitializer<TSecurityContextType> : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken);
}
