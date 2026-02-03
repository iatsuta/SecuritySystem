
namespace SecuritySystem.Expanders;

public class ExpandedRoleGroupSecurityRuleSetOptimizer(ISecurityRoleSource securityRoleSource) : IExpandedRoleGroupSecurityRuleSetOptimizer
{
    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Optimize(IEnumerable<DomainSecurityRule.ExpandedRolesSecurityRule> roleRules) =>
        this.Optimize([new DomainSecurityRule.ExpandedRoleGroupSecurityRule(roleRules)]);

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Optimize(IEnumerable<DomainSecurityRule.ExpandedRoleGroupSecurityRule> roleRuleGroups)
    {
        var request =

            from roleRuleGroup in roleRuleGroups

            from roleRule in roleRuleGroup.Children

            from role in roleRule.SecurityRoles

            let roleInfo = securityRoleSource.GetSecurityRole(role)

            group role by new
            {
                CustomCredential = roleRuleGroup.CustomCredential ?? roleRule.CustomCredential,
                CustomExpandType = roleRuleGroup.CustomExpandType ?? roleRule.CustomExpandType ?? roleInfo.Information.CustomExpandType,
                CustomRestriction = roleRuleGroup.CustomRestriction ?? roleRule.CustomRestriction ?? roleInfo.Information.Restriction
            }

            into g

            select new DomainSecurityRule.ExpandedRolesSecurityRule(g.Distinct())
            {
                CustomCredential = g.Key.CustomCredential,
                CustomExpandType = g.Key.CustomExpandType,
                CustomRestriction = g.Key.CustomRestriction
            };

        return new(request);
    }
}