

namespace SecuritySystem.Services;

public interface ISecurityRuleDeepOptimizer
{
    DomainSecurityRule Optimize(DomainSecurityRule securityRule);
}
