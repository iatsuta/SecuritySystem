

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
        return securityRule switch
        {
            DomainSecurityRule.AnyRoleSecurityRule => DomainSecurityRule.ExpandedRolesSecurityRule.Create(securityRoleSource.SecurityRoles)
                .TryApplyCustoms(securityRule),

            DomainSecurityRule.RoleGroupSecurityRule roleGroupSecurityRule => roleGroupSecurityRule.Children.Select(this.FullRoleExpand)
                .Aggregate(DomainSecurityRule.ExpandedRolesSecurityRule.Empty, (r1, r2) => r1 + r2)
                .TryApplyCustoms(securityRule),

            DomainSecurityRule.OperationSecurityRule operationSecurityRule => this.Expand(this.Expand(operationSecurityRule)),

            DomainSecurityRule.NonExpandedRolesSecurityRule nonExpandedRolesSecurityRule => this.Expand(nonExpandedRolesSecurityRule),

            DomainSecurityRule.ExpandedRolesSecurityRule expandedRolesSecurityRule => expandedRolesSecurityRule,

            DomainSecurityRule.RoleFactorySecurityRule dynamicRoleSecurityRule => this.FullRoleExpand(this.Expand(dynamicRoleSecurityRule)),

            _ => throw new ArgumentOutOfRangeException(nameof(securityRule))
        };
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
