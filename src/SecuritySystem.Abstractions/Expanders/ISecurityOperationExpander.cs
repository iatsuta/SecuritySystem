namespace SecuritySystem.Expanders;

public interface ISecurityOperationExpander
{
    DomainSecurityRule.NonExpandedRolesSecurityRule Expand(DomainSecurityRule.OperationSecurityRule securityRule);
}
