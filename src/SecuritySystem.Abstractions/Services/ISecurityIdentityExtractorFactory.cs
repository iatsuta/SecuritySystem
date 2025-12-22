namespace SecuritySystem.Services;

public interface ISecurityIdentityExtractorFactory
{
    ISecurityIdentityExtractor<TDomainObject> Create<TDomainObject>();
}