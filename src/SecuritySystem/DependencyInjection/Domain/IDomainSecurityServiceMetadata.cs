namespace SecuritySystem.DependencyInjection.Domain;

public interface IDomainSecurityServiceMetadata<TDomainObject> : IDomainSecurityServiceMetadata, IOverrideSecurityProviderFunctor<TDomainObject>
{
    static Type IDomainSecurityServiceMetadata.DomainType { get; } = typeof(TDomainObject);

    static abstract void Setup(IDomainSecurityServiceBuilder<TDomainObject> builder);
}

public interface IDomainSecurityServiceMetadata
{
    static abstract Type DomainType { get; }
}
