using SecuritySystem.Providers;

namespace SecuritySystem.DependencyInjection.Domain;

public interface IOverrideSecurityProviderFunctor<TDomainObject>
{
    ISecurityProvider<TDomainObject> OverrideSecurityProvider(ISecurityProvider<TDomainObject> baseProvider, SecurityRule.ModeSecurityRule securityRule) => baseProvider;

    ISecurityProvider<TDomainObject> OverrideSecurityProvider(ISecurityProvider<TDomainObject> baseProvider, DomainSecurityRule.OperationSecurityRule securityRule) => baseProvider;

    ISecurityProvider<TDomainObject> OverrideSecurityProvider(ISecurityProvider<TDomainObject> baseProvider, DomainSecurityRule.NonExpandedRolesSecurityRule securityRule) => baseProvider;

    ISecurityProvider<TDomainObject> OverrideSecurityProvider(ISecurityProvider<TDomainObject> baseProvider, DomainSecurityRule.ExpandedRolesSecurityRule securityRule) => baseProvider;
}
