using CommonFramework;

namespace SecuritySystem.GeneralPermission.Initialize;

public interface ISecurityContextInitializer<TSecurityContextType> : ISecurityContextInitializer
{
    new Task<MergeResult<TSecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken);
}

public interface ISecurityContextInitializer : ISecurityInitializer;