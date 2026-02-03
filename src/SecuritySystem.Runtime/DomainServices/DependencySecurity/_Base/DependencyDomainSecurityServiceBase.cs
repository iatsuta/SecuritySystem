using SecuritySystem.Expanders;
using SecuritySystem.Providers;

namespace SecuritySystem.DomainServices.DependencySecurity._Base;

public abstract class DependencyDomainSecurityServiceBase<TDomainObject, TBaseDomainObject>(
    ISecurityRuleExpander securityRuleExpander,
    IDomainSecurityService<TBaseDomainObject> baseDomainSecurityService)
    : DomainSecurityServiceBase<TDomainObject>
{
    protected override ISecurityProvider<TDomainObject> CreateSecurityProvider(SecurityRule securityRule)
    {
        if (securityRule is SecurityRule.ModeSecurityRule modeSecurityRule
            && securityRuleExpander.TryExpand(modeSecurityRule.ToDomain<TDomainObject>()) is { } customSecurityRule)
        {
            return this.CreateSecurityProvider(customSecurityRule);
        }
        else
        {
            return this.CreateDependencySecurityProvider(baseDomainSecurityService.GetSecurityProvider(securityRule));
        }
    }

    protected abstract ISecurityProvider<TDomainObject> CreateDependencySecurityProvider(ISecurityProvider<TBaseDomainObject> baseProvider);
}
