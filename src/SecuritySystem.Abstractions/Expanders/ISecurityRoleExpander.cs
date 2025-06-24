namespace SecuritySystem.Expanders;

public interface ISecurityRoleExpander
{
    DomainSecurityRule.ExpandedRolesSecurityRule Expand(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule);
}
