using CommonFramework;


namespace SecuritySystem.GeneralPermission.Initialize;

public interface IGeneralSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
