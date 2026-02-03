using CommonFramework.DictionaryCache;
using SecuritySystem.Providers;

namespace SecuritySystem.DomainServices;

public abstract class DomainSecurityServiceBase<TDomainObject> : IDomainSecurityService<TDomainObject>
{
    private readonly IDictionaryCache<SecurityRule, ISecurityProvider<TDomainObject>> providersCache;

    protected DomainSecurityServiceBase() =>
        this.providersCache = new DictionaryCache<SecurityRule, ISecurityProvider<TDomainObject>>(securityRule =>
        {
            return this.CreateSecurityProvider(securityRule)
                       .OverrideAccessDeniedResult(accessDeniedResult => accessDeniedResult with { SecurityRule = securityRule });
        }).WithLock();

    protected abstract ISecurityProvider<TDomainObject> CreateSecurityProvider(SecurityRule securityRule);

    public ISecurityProvider<TDomainObject> GetSecurityProvider(SecurityRule securityRule) => this.providersCache[securityRule];
}
