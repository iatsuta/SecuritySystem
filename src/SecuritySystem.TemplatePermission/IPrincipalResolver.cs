using Framework.Authorization.Domain;

using SecuritySystem.Credential;

namespace SecuritySystem.TemplatePermission;

public interface IPrincipalResolver
{
    Task<Principal> Resolve(UserCredential userCredential, CancellationToken cancellationToken = default);
}
