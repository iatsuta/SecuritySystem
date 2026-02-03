using SecuritySystem.Builders._Factory;
using SecuritySystem.Expanders;
using SecuritySystem.Providers;

namespace SecuritySystem.Services;

public class RoleBaseSecurityProviderFactory<TDomainObject>(
    ISecurityFilterFactory<TDomainObject> securityFilterFactory,
    IAccessorsFilterFactory<TDomainObject> accessorsFilterFactory,
    ISecurityRuleExpander securityRuleExpander,
    ISecurityPathRestrictionService securityPathRestrictionService) : IRoleBaseSecurityProviderFactory<TDomainObject>
{
    public virtual ISecurityProvider<TDomainObject> Create(DomainSecurityRule.RoleBaseSecurityRule securityRule, SecurityPath<TDomainObject> securityPath) =>

        securityRuleExpander
            .FullRoleExpand(securityRule)
            .GetActualChildren()
            .Select(innerSecurityRule => this.Create(innerSecurityRule, securityPath))
            .Or();

    private ISecurityProvider<TDomainObject> Create(DomainSecurityRule.ExpandedRolesSecurityRule securityRule, SecurityPath<TDomainObject> securityPath) =>

        new RoleBaseSecurityPathProvider<TDomainObject>(
            securityFilterFactory,
            accessorsFilterFactory,
            securityRule,
            securityPathRestrictionService.ApplyRestriction(securityPath, securityRule.CustomRestriction ?? SecurityPathRestriction.Default));
}