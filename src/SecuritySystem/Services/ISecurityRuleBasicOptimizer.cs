namespace SecuritySystem.Services;

public interface ISecurityRuleBasicOptimizer
{
    DomainSecurityRule Optimize(DomainSecurityRule securityRule);
}
