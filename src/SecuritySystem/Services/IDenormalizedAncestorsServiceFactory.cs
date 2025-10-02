namespace SecuritySystem.Services;

public interface IDenormalizedAncestorsServiceFactory
{
    IDenormalizedAncestorsService<TDomainObject> Create<TDomainObject>();
}