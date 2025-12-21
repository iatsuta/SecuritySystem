namespace SecuritySystem.Services;

public interface ISecurityIdentityExtractor<in TDomainObject>
{
    ISecurityIdentityConverter Converter { get; }

    TypedSecurityIdentity Extract(TDomainObject domainObject);
}

public interface ISecurityIdentityExtractor
{
    TypedSecurityIdentity Extract<TDomainObject>(TDomainObject domainObject);

    TypedSecurityIdentity? TryConvert<TDomainObject>(SecurityIdentity securityIdentity);
}