namespace SecuritySystem.Expanders;

public interface IExpandedRoleGroupSecurityRuleSetOptimizer
{
    DomainSecurityRule.ExpandedRoleGroupSecurityRule Optimize(IEnumerable<DomainSecurityRule.ExpandedRolesSecurityRule> roleRules);

    DomainSecurityRule.ExpandedRoleGroupSecurityRule Optimize(IEnumerable<DomainSecurityRule.ExpandedRoleGroupSecurityRule> roleRules);
}