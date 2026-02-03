using CommonFramework;

using SecuritySystem.SecurityRuleInfo;

namespace SecuritySystem.Expanders;

public class SecurityModeExpander(IEnumerable<DomainModeSecurityRuleInfo> infoList) : ISecurityModeExpander
{
    private readonly IReadOnlyDictionary<DomainSecurityRule.DomainModeSecurityRule, DomainSecurityRule> dict =
        infoList.Select(info => (info.SecurityRule, info.Implementation)).ToDictionary();

    public DomainSecurityRule? TryExpand(DomainSecurityRule.DomainModeSecurityRule securityRule)
    {
        return this.dict.GetValueOrDefault(securityRule.WithDefaultCredential()).Maybe(v => v with { CustomCredential = securityRule.CustomCredential });
    }
}