namespace SecuritySystem.SecurityRuleInfo;

public interface IClientDomainModeSecurityRuleSource
{
    IEnumerable<DomainSecurityRule.DomainModeSecurityRule> GetRules();
}
