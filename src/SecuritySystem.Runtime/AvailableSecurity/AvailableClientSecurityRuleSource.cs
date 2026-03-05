using SecuritySystem.SecurityRuleInfo;

namespace SecuritySystem.AvailableSecurity;

public class AvailableClientSecurityRuleSource(
    IAvailableSecurityRoleSource availableSecurityRoleSource,
    IClientSecurityRuleResolver clientSecurityRuleResolver) : IAvailableClientSecurityRuleSource
{
    public IAsyncEnumerable<DomainSecurityRule.ClientSecurityRule> GetAvailableSecurityRules() =>
        availableSecurityRoleSource
            .GetAvailableSecurityRoles()
            .SelectMany(securityRole => clientSecurityRuleResolver.Resolve(securityRole))
            .Distinct();
}