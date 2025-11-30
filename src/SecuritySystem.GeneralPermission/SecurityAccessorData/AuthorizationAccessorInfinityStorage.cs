


using SecuritySystem.Attributes;
using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.GeneralPermission;

public class TemplateAccessorInfinityStorage([DisabledSecurity] IRepository<TPrincipal> principalRepository)
    : ISecurityAccessorInfinityStorage
{
    public IEnumerable<string> GetInfinityData() => principalRepository.GetQueryable().Select(p => p.Name);
}
