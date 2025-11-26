


using SecuritySystem.Attributes;
using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationAccessorInfinityStorage([DisabledSecurity] IRepository<TPrincipal> principalRepository)
    : ISecurityAccessorInfinityStorage
{
    public IEnumerable<string> GetInfinityData() => principalRepository.GetQueryable().Select(p => p.Name);
}
