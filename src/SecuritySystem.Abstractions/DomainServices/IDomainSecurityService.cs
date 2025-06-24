using SecuritySystem.Providers;

namespace SecuritySystem.DomainServices;

public interface IDomainSecurityService<TDomainObject>
{
    ISecurityProvider<TDomainObject> GetSecurityProvider(SecurityRule securityRule);
}
