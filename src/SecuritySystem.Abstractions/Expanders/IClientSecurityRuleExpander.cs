namespace SecuritySystem.Expanders;

public interface IClientSecurityRuleExpander
{
    DomainSecurityRule Expand(DomainSecurityRule.ClientSecurityRule securityRule);
}
