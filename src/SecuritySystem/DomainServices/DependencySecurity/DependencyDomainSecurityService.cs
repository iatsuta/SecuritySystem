using CommonFramework.GenericRepository;

using SecuritySystem.DomainServices.DependencySecurity._Base;
using SecuritySystem.Expanders;
using SecuritySystem.Providers;
using SecuritySystem.Providers.DependencySecurity;
using SecuritySystem.RelativeDomainPathInfo;

namespace SecuritySystem.DomainServices.DependencySecurity;

public class DependencyDomainSecurityService<TDomainObject, TBaseDomainObject>(
    ISecurityRuleExpander securityRuleExpander,
    IDomainSecurityService<TBaseDomainObject> baseDomainSecurityService,
    IQueryableSource queryableSource,
    IRelativeDomainPathInfo<TDomainObject, TBaseDomainObject> relativeDomainPathInfo)
    : DependencyDomainSecurityServiceBase<TDomainObject, TBaseDomainObject>(
        securityRuleExpander,
        baseDomainSecurityService)
    where TBaseDomainObject : class
{
    protected override ISecurityProvider<TDomainObject> CreateDependencySecurityProvider(ISecurityProvider<TBaseDomainObject> baseProvider)
    {
        return new DependencySecurityProvider<TDomainObject, TBaseDomainObject>(
            baseProvider,
            relativeDomainPathInfo,
            queryableSource);
    }
}
