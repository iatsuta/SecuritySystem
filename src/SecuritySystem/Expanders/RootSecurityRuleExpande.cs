

using SecuritySystem.Services;

namespace SecuritySystem.Expanders;

public class RootSecurityRuleExpander(
    ISecurityModeExpander securityModeExpander,
    ISecurityOperationExpander securityOperationExpander,
    ISecurityRoleExpander securityRoleExpander,
    IRoleFactorySecurityRuleExpander roleFactorySecurityRuleExpander,
    ISecurityRoleSource securityRoleSource,
    IClientSecurityRuleExpander clientSecurityRuleExpander,
    ISecurityRuleHeaderExpander securityRuleHeaderExpander)
    : ISecurityRuleExpander
{
    public DomainSecurityRule? TryExpand(DomainSecurityRule.DomainModeSecurityRule securityRule)
    {
        return securityModeExpander.TryExpand(securityRule);
    }

    public DomainSecurityRule Expand(DomainSecurityRule.SecurityRuleHeader securityRuleHeader) => securityRuleHeaderExpander.Expand(securityRuleHeader);

    public DomainSecurityRule Expand(DomainSecurityRule.ClientSecurityRule securityRule)
    {
        return clientSecurityRuleExpander.Expand(securityRule);
    }

    public DomainSecurityRule.NonExpandedRolesSecurityRule Expand(DomainSecurityRule.OperationSecurityRule securityRule)
    {
        return securityOperationExpander.Expand(securityRule);
    }

    public DomainSecurityRule.ExpandedRolesSecurityRule Expand(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule)
    {
        return securityRoleExpander.Expand(securityRule);
    }

    public DomainSecurityRule.RoleBaseSecurityRule Expand(DomainSecurityRule.RoleFactorySecurityRule securityRule)
    {
        return roleFactorySecurityRuleExpander.Expand(securityRule);
    }

    public DomainSecurityRule.ExpandedRolesSecurityRule FullRoleExpand(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        switch (securityRule)
        {
            case DomainSecurityRule.AnyRoleSecurityRule:

                return DomainSecurityRule.ExpandedRolesSecurityRule.Create(securityRoleSource.SecurityRoles).TryApplyCustoms(securityRule);

            case DomainSecurityRule.RoleGroupSecurityRule roleGroupSecurityRule:
                return roleGroupSecurityRule.Children.Select(this.FullRoleExpand)
                                            .Aggregate(DomainSecurityRule.ExpandedRolesSecurityRule.Empty, (r1, r2) => r1 + r2)
                                            .TryApplyCustoms(securityRule);

            case DomainSecurityRule.OperationSecurityRule operationSecurityRule:
                return this.Expand(this.Expand(operationSecurityRule));

            case DomainSecurityRule.NonExpandedRolesSecurityRule nonExpandedRolesSecurityRule:
                return this.Expand(nonExpandedRolesSecurityRule);

            case DomainSecurityRule.ExpandedRolesSecurityRule expandedRolesSecurityRule:
                return expandedRolesSecurityRule;

            case DomainSecurityRule.RoleFactorySecurityRule dynamicRoleSecurityRule:
                return this.FullRoleExpand(this.Expand(dynamicRoleSecurityRule));

            default:
                throw new ArgumentOutOfRangeException(nameof(securityRule));
        }
    }

    public DomainSecurityRule FullDomainExpand(DomainSecurityRule securityRule)
    {
        return new FullDomainExpandVisitor(this).Visit(securityRule);
    }

    private class FullDomainExpandVisitor(ISecurityRuleExpander expander)
        : SecurityRuleVisitor
    {
        protected override DomainSecurityRule Visit(DomainSecurityRule.RoleBaseSecurityRule baseSecurityRule) => expander.FullRoleExpand(baseSecurityRule);

        protected override DomainSecurityRule Visit(DomainSecurityRule.DomainModeSecurityRule securityRule) => this.Visit(expander.Expand(securityRule));

        protected override DomainSecurityRule Visit(DomainSecurityRule.SecurityRuleHeader securityRule) => this.Visit(expander.Expand(securityRule));

        protected override DomainSecurityRule Visit(DomainSecurityRule.ClientSecurityRule securityRule) => this.Visit(expander.Expand(securityRule));
    }
}
