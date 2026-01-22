namespace SecuritySystem.Expanders;

public interface ISecurityRoleGroupExpander
{
    DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRoleGroupSecurityRule securityRule);

    DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule);
}