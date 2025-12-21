namespace SecuritySystem.Services;

public interface ISecurityIdentityExtractor<in TDomainObject>
{
    ISecurityIdentityConverter Converter { get; }

    TypedSecurityIdentity Extract(TDomainObject domainObject);
}