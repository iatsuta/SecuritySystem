using CommonFramework.IdentitySource;

using Microsoft.Extensions.DependencyInjection;

using System.Collections.Concurrent;

namespace SecuritySystem.Services;

public class SecurityIdentityExtractor<TDomainObject>(IServiceProvider serviceProvider, IIdentityInfoSource identityInfoSource)
    : ISecurityIdentityExtractor<TDomainObject>
{
    private readonly Lazy<ISecurityIdentityExtractor<TDomainObject>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TDomainObject>();

        var innerServiceType = typeof(SecurityIdentityExtractor<,>).MakeGenericType(identityInfo.DomainObjectType, identityInfo.IdentityType);

        return (ISecurityIdentityExtractor<TDomainObject>)ActivatorUtilities.CreateInstance(
            serviceProvider,
            innerServiceType,
            identityInfo);
    });

    public ISecurityIdentityConverter Converter => this.lazyInnerService.Value.Converter;

    public TypedSecurityIdentity Extract(TDomainObject domainObject) => this.lazyInnerService.Value.Extract(domainObject);
}

public class SecurityIdentityExtractor<TDomainObject, TDomainObjectIdent>(ISecurityIdentityConverter<TDomainObjectIdent> converter, IdentityInfo<TDomainObject, TDomainObjectIdent> identityInfo)
    : ISecurityIdentityExtractor<TDomainObject>
    where TDomainObjectIdent : notnull
{
    public ISecurityIdentityConverter Converter { get; } = converter;

    public TypedSecurityIdentity Extract(TDomainObject domainObject) => TypedSecurityIdentity.Create(identityInfo.Id.Getter(domainObject));
}

public class SecurityIdentityExtractor(IServiceProvider serviceProvider) : ISecurityIdentityExtractor
{
    private readonly ConcurrentDictionary<Type, object> cache = new();

    public TypedSecurityIdentity Extract<TDomainObject>(TDomainObject domainObject) =>
        this.GetExtractor<TDomainObject>().Extract(domainObject);

    public TypedSecurityIdentity? TryConvert<TDomainObject>(SecurityIdentity securityIdentity) =>
        this.GetExtractor<TDomainObject>().Converter.TryConvert(securityIdentity);

    private ISecurityIdentityExtractor<TDomainObject> GetExtractor<TDomainObject>()
    {
        return (ISecurityIdentityExtractor<TDomainObject>)this.cache.GetOrAdd(
            typeof(TDomainObject),
            _ => serviceProvider.GetRequiredService<ISecurityIdentityExtractor<TDomainObject>>());
    }
}