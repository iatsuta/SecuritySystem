

using SecuritySystem.Credential;

namespace SecuritySystem.TemplatePermission;

public interface IPrincipalResolver<TPrincipal>
{
    Task<TPrincipal> Resolve(UserCredential userCredential, CancellationToken cancellationToken = default);
}
