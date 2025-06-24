using SecuritySystem.DomainServices.DependencySecurity._Base;
using SecuritySystem.Expanders;
using SecuritySystem.Providers;
using SecuritySystem.Providers.DependencySecurity;
using SecuritySystem.Services;

namespace SecuritySystem.DomainServices.DependencySecurity;

public class UntypedDependencyDomainSecurityService<TDomainObject, TBaseDomainObject>(
    ISecurityRuleExpander securityRuleExpander,
    IDomainSecurityService<TBaseDomainObject> baseDomainSecurityService,
    IQueryableSource queryableSource)
    : DependencyDomainSecurityServiceBase<TDomainObject, TBaseDomainObject>(
        securityRuleExpander,
        baseDomainSecurityService)
    where TDomainObject : IIdentityObject<Guid>
    where TBaseDomainObject : class, IIdentityObject<Guid>
{
    protected override ISecurityProvider<TDomainObject> CreateDependencySecurityProvider(ISecurityProvider<TBaseDomainObject> baseProvider)
    {
        return new UntypedDependencySecurityProvider<TDomainObject, TBaseDomainObject>(
            baseProvider,
            queryableSource);
    }
}
