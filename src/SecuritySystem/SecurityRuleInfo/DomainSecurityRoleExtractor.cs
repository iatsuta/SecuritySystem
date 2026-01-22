using System.Collections.Concurrent;

using SecuritySystem.Expanders;
using SecuritySystem.Services;

namespace SecuritySystem.SecurityRuleInfo;

public class DomainSecurityRoleExtractor(ISecurityRuleExpander expander, IExpandedRoleGroupSecurityRuleSetOptimizer securityRuleSetOptimizer)
    : IDomainSecurityRoleExtractor
{
    private readonly ConcurrentDictionary<DomainSecurityRule, DomainSecurityRule.ExpandedRoleGroupSecurityRule> rulesCache = new();

    private readonly ConcurrentDictionary<DomainSecurityRule, IReadOnlySet<SecurityRole>> rolesCache = new();

    public IEnumerable<SecurityRole> ExtractSecurityRoles(DomainSecurityRule securityRule) =>
        this.rolesCache.GetOrAdd(securityRule.WithDefaultCredential(), _ =>
            expander.FullRoleExpand(this.rulesCache[securityRule]).Children.SelectMany(c => c.SecurityRoles).ToHashSet());

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule ExtractSecurityRule(DomainSecurityRule securityRule) =>
        this.rulesCache.GetOrAdd(securityRule, _ =>
        {
            var usedRules = new HashSet<DomainSecurityRule.ExpandedRoleGroupSecurityRule>();

            new ScanVisitor(usedRules).Visit(expander.FullDomainExpand(securityRule));

            return securityRuleSetOptimizer.Optimize(usedRules);
        });


    private class ScanVisitor(ISet<DomainSecurityRule.ExpandedRoleGroupSecurityRule> usedRules) : SecurityRuleVisitor
    {
        protected override DomainSecurityRule Visit(DomainSecurityRule.ExpandedRoleGroupSecurityRule securityRule)
        {
            usedRules.Add(securityRule);

            return securityRule;
        }
    }
}