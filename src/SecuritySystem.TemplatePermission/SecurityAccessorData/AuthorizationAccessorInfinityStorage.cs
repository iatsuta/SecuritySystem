using Framework.Authorization.Domain;
using Framework.DomainDriven.Repository;

using SecuritySystem.Attributes;
using SecuritySystem.SecurityAccessor;

namespace SecuritySystem.TemplatePermission;

public class AuthorizationAccessorInfinityStorage([DisabledSecurity] IRepository<Principal> principalRepository)
    : ISecurityAccessorInfinityStorage
{
    public IEnumerable<string> GetInfinityData() => principalRepository.GetQueryable().Select(p => p.Name);
}
