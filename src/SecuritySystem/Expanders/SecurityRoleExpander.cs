using CommonFramework;

using System.Collections.Concurrent;

namespace SecuritySystem.Expanders;

public class SecurityRoleGroupExpander(ISecurityRoleSource securityRoleSource, IExpandedRoleGroupSecurityRuleSetOptimizer setOptimizer)
    : ISecurityRoleGroupExpander
{
    private readonly ConcurrentDictionary<DomainSecurityRule.NonExpandedRoleGroupSecurityRule, DomainSecurityRule.ExpandedRoleGroupSecurityRule> cache = new();

    private readonly ConcurrentDictionary<DomainSecurityRule.NonExpandedRolesSecurityRule, DomainSecurityRule.ExpandedRoleGroupSecurityRule> innerCache = new();

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRoleGroupSecurityRule baseSecurityRule) =>

        baseSecurityRule.WithDefaultCredential(securityRule =>

            this.cache.GetOrAdd(securityRule, _ =>
            {
                var request =

                    from c in securityRule.Children

                    let g = this.Expand(c)

                    from r in g.Children

                    select r.ApplyCustoms(g);

                return setOptimizer.Optimize(request);
            }));

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRolesSecurityRule baseSecurityRule) =>

        baseSecurityRule.WithDefaultCredential(securityRule =>

            this.innerCache.GetOrAdd(securityRule, _ =>
            {
                var otherSecurityRoles = securityRoleSource.SecurityRoles.Where(sr =>
                        sr.GetAllElements(c => c.Information.Children.Select(securityRoleSource.GetSecurityRole)).IsIntersected(securityRule.SecurityRoles))
                    .Distinct()
                    .Except(securityRule.SecurityRoles);

                return setOptimizer.Optimize([
                    new DomainSecurityRule.ExpandedRolesSecurityRule(securityRule.SecurityRoles).ApplyCustoms(securityRule),
                    new(otherSecurityRoles)
                ]);
            }));
}