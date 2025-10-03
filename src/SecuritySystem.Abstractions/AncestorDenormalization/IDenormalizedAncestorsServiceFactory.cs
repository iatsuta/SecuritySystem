namespace SecuritySystem.AncestorDenormalization;

public interface IDenormalizedAncestorsServiceFactory
{
    IDenormalizedAncestorsService<TDomainObject> Create<TDomainObject>();
}