using CommonFramework;

namespace SecuritySystem.ExternalSystem.ApplicationSecurity;

public class SecurityAdministratorRuleFactory(SecurityAdministratorRuleInfo securityAdministratorRuleInfo)
    : IFactory<DomainSecurityRule.RoleBaseSecurityRule>
{
    public DomainSecurityRule.RoleBaseSecurityRule Create() => securityAdministratorRuleInfo.SecurityRole;
}
