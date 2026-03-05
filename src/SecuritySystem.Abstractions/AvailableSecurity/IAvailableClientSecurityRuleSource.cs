namespace SecuritySystem.AvailableSecurity;

public interface IAvailableClientSecurityRuleSource
{
    IAsyncEnumerable<DomainSecurityRule.ClientSecurityRule> GetAvailableSecurityRules();
}