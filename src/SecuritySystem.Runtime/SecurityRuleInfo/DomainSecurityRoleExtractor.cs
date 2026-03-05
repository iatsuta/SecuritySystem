using SecuritySystem.Expanders;
using SecuritySystem.Services;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace SecuritySystem.SecurityRuleInfo;

public class DomainSecurityRoleExtractor(ISecurityRuleExpander expander, IExpandedRoleGroupSecurityRuleSetOptimizer securityRuleSetOptimizer)
    : IDomainSecurityRoleExtractor
{
    private readonly ConcurrentDictionary<DomainSecurityRule, DomainSecurityRule.ExpandedRoleGroupSecurityRule> rulesCache = new();

    private readonly ConcurrentDictionary<DomainSecurityRule, ImmutableHashSet<SecurityRole>> rolesCache = new();

    public ImmutableHashSet<SecurityRole> ExtractSecurityRoles(DomainSecurityRule securityRule) =>
        this.rolesCache.GetOrAdd(securityRule.WithDefaultCredential(), _ =>
            expander.FullRoleExpand(this.ExtractSecurityRule(securityRule)).Children.SelectMany(c => c.SecurityRoles).ToImmutableHashSet());

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule ExtractSecurityRule(DomainSecurityRule securityRule) =>
        this.rulesCache.GetOrAdd(securityRule.WithDefaultCredential(), _ =>
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