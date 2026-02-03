using SecuritySystem.Services;

namespace SecuritySystem.Expanders;

public class RootSecurityRuleExpander(
    ISecurityModeExpander securityModeExpander,
    ISecurityOperationExpander securityOperationExpander,
    ISecurityRoleGroupExpander securityRoleGroupExpander,
    IRoleFactorySecurityRuleExpander roleFactorySecurityRuleExpander,
    ISecurityRoleSource securityRoleSource,
    IClientSecurityRuleExpander clientSecurityRuleExpander,
    ISecurityRuleHeaderExpander securityRuleHeaderExpander)
    : ISecurityRuleExpander
{
    private readonly Lazy<DomainSecurityRule.ExpandedRoleGroupSecurityRule> anyRoleExpandedSecurityRule = new(() =>

        new ([new(securityRoleSource.SecurityRoles)]));

    public DomainSecurityRule? TryExpand(DomainSecurityRule.DomainModeSecurityRule securityRule)
    {
        return securityModeExpander.TryExpand(securityRule);
    }

    public DomainSecurityRule Expand(DomainSecurityRule.SecurityRuleHeader securityRuleHeader) => securityRuleHeaderExpander.Expand(securityRuleHeader);

    public DomainSecurityRule Expand(DomainSecurityRule.ClientSecurityRule securityRule) =>
        clientSecurityRuleExpander.Expand(securityRule);

    public DomainSecurityRule.NonExpandedRolesSecurityRule Expand(DomainSecurityRule.OperationSecurityRule securityRule) =>
        securityOperationExpander.Expand(securityRule);

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRoleGroupSecurityRule securityRule) =>
        securityRoleGroupExpander.Expand(securityRule);

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule Expand(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule) =>
        securityRoleGroupExpander.Expand(securityRule);

    public DomainSecurityRule.RoleBaseSecurityRule Expand(DomainSecurityRule.RoleFactorySecurityRule securityRule)
    {
        return roleFactorySecurityRuleExpander.Expand(securityRule);
    }

    public DomainSecurityRule.ExpandedRoleGroupSecurityRule FullRoleExpand(DomainSecurityRule.RoleBaseSecurityRule securityRule)
    {
        return securityRule switch
        {
            DomainSecurityRule.AnyRoleSecurityRule => this.anyRoleExpandedSecurityRule.Value.ApplyCustoms(securityRule),

            DomainSecurityRule.ExpandedRoleGroupSecurityRule expandedRoleGroupSecurityRule => expandedRoleGroupSecurityRule,

            DomainSecurityRule.NonExpandedRoleGroupSecurityRule roleGroupSecurityRule => this.Expand(roleGroupSecurityRule),

            DomainSecurityRule.OperationSecurityRule operationSecurityRule => this.Expand(this.Expand(operationSecurityRule)),

            DomainSecurityRule.NonExpandedRolesSecurityRule nonExpandedRolesSecurityRule => this.Expand(nonExpandedRolesSecurityRule),

            DomainSecurityRule.ExpandedRolesSecurityRule expandedRolesSecurityRule => new([expandedRolesSecurityRule]),

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
