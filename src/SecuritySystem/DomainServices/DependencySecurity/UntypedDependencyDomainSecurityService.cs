using CommonFramework.IdentitySource;
using Microsoft.Extensions.DependencyInjection;

using SecuritySystem.DomainServices.DependencySecurity._Base;
using SecuritySystem.Expanders;

using SecuritySystem.Providers;
using SecuritySystem.Providers.DependencySecurity;

namespace SecuritySystem.DomainServices.DependencySecurity;

public class UntypedDependencyDomainSecurityService<TDomainObject, TBaseDomainObject>(
    IServiceProvider serviceProvider,
    ISecurityRuleExpander securityRuleExpander,
    IDomainSecurityService<TBaseDomainObject> baseDomainSecurityService,
    IIdentityInfoSource identityInfoSource)
    : DependencyDomainSecurityServiceBase<TDomainObject, TBaseDomainObject>(
        securityRuleExpander,
        baseDomainSecurityService)
{
    protected override ISecurityProvider<TDomainObject> CreateDependencySecurityProvider(ISecurityProvider<TBaseDomainObject> baseProvider)
    {
        var domainIdentityInfo = identityInfoSource.GetIdentityInfo(typeof(TDomainObject));
        var baseDomainIdentityInfo = identityInfoSource.GetIdentityInfo(typeof(TBaseDomainObject));

        var securityProviderType = typeof(UntypedDependencySecurityProvider<,,>)
            .MakeGenericType(typeof(TDomainObject), typeof(TBaseDomainObject), domainIdentityInfo.IdentityType);

        return (ISecurityProvider<TDomainObject>)ActivatorUtilities.CreateInstance(serviceProvider, securityProviderType, baseProvider, domainIdentityInfo,
            baseDomainIdentityInfo);
    }
}