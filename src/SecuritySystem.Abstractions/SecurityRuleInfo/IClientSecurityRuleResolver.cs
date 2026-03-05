using System.Collections.Immutable;

namespace SecuritySystem.SecurityRuleInfo;

public interface IClientSecurityRuleResolver
{
    ImmutableArray<DomainSecurityRule.ClientSecurityRule> Resolve(SecurityRole securityRole);
}
