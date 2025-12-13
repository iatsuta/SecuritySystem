using CommonFramework.GenericRepository;
using CommonFramework.RelativePath;

using SecuritySystem.DomainServices.DependencySecurity._Base;
using SecuritySystem.Expanders;
using SecuritySystem.Providers;
using SecuritySystem.Providers.DependencySecurity;

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
