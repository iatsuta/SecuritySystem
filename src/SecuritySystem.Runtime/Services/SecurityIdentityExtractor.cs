using CommonFramework;
using CommonFramework.IdentitySource;

namespace SecuritySystem.Services;

public class SecurityIdentityExtractor<TDomainObject>(
    IServiceProxyFactory serviceProxyFactory,
    IIdentityInfoSource identityInfoSource)
    : ISecurityIdentityExtractor<TDomainObject>
{
    private readonly Lazy<ISecurityIdentityExtractor<TDomainObject>> lazyInnerService = new(() =>
    {
        var identityInfo = identityInfoSource.GetIdentityInfo<TDomainObject>();

        var innerServiceType = typeof(SecurityIdentityExtractor<,>).MakeGenericType(identityInfo.DomainObjectType, identityInfo.IdentityType);

        return serviceProxyFactory.Create<ISecurityIdentityExtractor<TDomainObject>>(
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