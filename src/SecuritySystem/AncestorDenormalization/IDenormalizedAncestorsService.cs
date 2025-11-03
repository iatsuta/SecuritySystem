namespace SecuritySystem.AncestorDenormalization;

public interface IDenormalizedAncestorsService<in TDomainObject, in TDirectAncestorLink> : IDenormalizedAncestorsService<TDomainObject>;