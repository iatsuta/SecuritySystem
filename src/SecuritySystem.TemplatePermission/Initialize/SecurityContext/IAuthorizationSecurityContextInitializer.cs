using CommonFramework;

using Framework.Authorization.Domain;

using SecuritySystem;

namespace SecuritySystem.TemplatePermission.Initialize;

public interface IAuthorizationSecurityContextInitializer : ISecurityInitializer
{
    new Task<MergeResult<SecurityContextType, SecurityContextInfo>> Init(CancellationToken cancellationToken = default);
}
