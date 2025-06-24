using SecuritySystem.Expanders;
using SecuritySystem.Providers;


namespace SecuritySystem.DomainServices;

public abstract class DomainSecurityService<TDomainObject>(ISecurityRuleExpander securityRuleExpander) : DomainSecurityServiceBase<TDomainObject>
{
    protected sealed override ISecurityProvider<TDomainObject> CreateSecurityProvider(SecurityRule baseSecurityRule)
    {
        switch (baseSecurityRule)
        {
            case SecurityRule.ModeSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRule);

            case DomainSecurityRule.DomainModeSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRuleExpander.Expand(securityRule));

            case DomainSecurityRule.ClientSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRuleExpander.Expand(securityRule));

            case DomainSecurityRule.OperationSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRule);

            case DomainSecurityRule.NonExpandedRolesSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRule);

            case DomainSecurityRule.ExpandedRolesSecurityRule securityRule:
                return this.CreateSecurityProvider(securityRule);

            case DomainSecurityRule securityRule:
                return this.CreateFinalSecurityProvider(securityRule);

            default:
                throw new ArgumentOutOfRangeException(nameof(baseSecurityRule));
        }
    }

    protected virtual ISecurityProvider<TDomainObject> CreateSecurityProvider(SecurityRule.ModeSecurityRule securityRule)
    {
        return this.GetSecurityProvider(securityRule.ToDomain<TDomainObject>());
    }

    protected virtual ISecurityProvider<TDomainObject> CreateSecurityProvider(DomainSecurityRule.OperationSecurityRule securityRule)
    {
        return this.GetSecurityProvider(securityRuleExpander.Expand(securityRule));
    }

    protected virtual ISecurityProvider<TDomainObject> CreateSecurityProvider(DomainSecurityRule.NonExpandedRolesSecurityRule securityRule)
    {
        return this.GetSecurityProvider(securityRuleExpander.Expand(securityRule));
    }

    protected virtual ISecurityProvider<TDomainObject> CreateSecurityProvider(DomainSecurityRule.ExpandedRolesSecurityRule securityRule)
    {
        return this.CreateFinalSecurityProvider(securityRule);
    }

    protected abstract ISecurityProvider<TDomainObject> CreateFinalSecurityProvider(DomainSecurityRule securityRule);
}
