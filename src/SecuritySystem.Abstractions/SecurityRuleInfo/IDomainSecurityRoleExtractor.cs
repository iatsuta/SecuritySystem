namespace SecuritySystem.SecurityRuleInfo;

public interface IDomainSecurityRoleExtractor
{
    IEnumerable<SecurityRole> ExtractSecurityRoles(DomainSecurityRule securityRule);

    DomainSecurityRule.ExpandedRoleGroupSecurityRule ExtractSecurityRule(DomainSecurityRule securityRule);
}
